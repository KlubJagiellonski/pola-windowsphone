using Lumia.Imaging;
using Pola.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VideoEffects;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ZXing;
using ZXing.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Pola.View.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Scanner : Page
    {
        private NavigationHelper navigationHelper;

        DisplayRequest displayRequest = new DisplayRequest();
        MediaCapture m_capture;
        ContinuousAutoFocus m_autoFocus;
        bool m_initializing;

        BarcodeReader m_reader = new BarcodeReader
        {
            Options = new DecodingOptions
            {
                PossibleFormats = new BarcodeFormat[] { BarcodeFormat.All_1D },
                TryHarder = true
            }
        };
        Stopwatch m_time = new Stopwatch();
        volatile bool m_snapRequested;

        public Scanner()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            // Prevent screen timeout
            displayRequest.RequestActive();

            Application.Current.Resuming += OnResuming;
            Application.Current.Suspending += OnSuspending;
            Window.Current.VisibilityChanged += Current_VisibilityChanged;

            var ignore = InitializeCaptureAsync();
        }

        private void UpdateOverlaySize()
        {
            double scaleX = this.ActualWidth / Overlay.ActualWidth;
            double scaleY = this.ActualHeight / Overlay.ActualHeight;

            Overlay.RenderTransformOrigin = new Point(0.5, 0.5);
            Overlay.RenderTransform = new ScaleTransform()
            {
                ScaleX = scaleX,
                ScaleY = scaleY,
            };
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);

            Application.Current.Resuming -= OnResuming;
            Application.Current.Suspending -= OnSuspending;
            Window.Current.VisibilityChanged -= Current_VisibilityChanged;

            displayRequest.RequestRelease();

            await DisposeCaptureAsync();
        }

        private void OnResuming(object sender, object e)
        {
            // Dispatch call to the UI thread since the event may get fired on some other thread
            var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await InitializeCaptureAsync();
            });
        }

        private void OnSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            // Dispatch call to the UI thread since the event may get fired on some other thread
            var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await DisposeCaptureAsync();
            });
        }

        async void Current_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            if (e.Visible)
            {
                await InitializeCaptureAsync();
            }
            else
            {
                await DisposeCaptureAsync();
            }
        }

        void capture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            // Dispatch call to the UI thread since the event may get fired on some other thread
            var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await DisposeCaptureAsync();
            });
        }

        private async Task InitializeCaptureAsync()
        {
            if (m_initializing || (m_capture != null))
            {
                return;
            }
            m_initializing = true;

            try
            {
                var settings = new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = await GetBackOrDefaulCameraIdAsync(),
                    StreamingCaptureMode = StreamingCaptureMode.Video
                };

                var capture = new MediaCapture();
                await capture.InitializeAsync(settings);
                capture.SetPreviewRotation(VideoRotation.Clockwise90Degrees);

                // Select the capture resolution closest to screen resolution
                var formats = capture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);
                var format = (VideoEncodingProperties)formats.OrderBy((item) =>
                {
                    var props = (VideoEncodingProperties)item;
                    return Math.Abs(props.Width - this.ActualHeight) + Math.Abs(props.Height - this.ActualWidth);
                }).First();

                Debug.WriteLine("{0} x {1}", format.Width, format.Height);
                
                await capture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, format);

                // Make the preview full screen
                var scale = Math.Min(this.ActualWidth / format.Height, this.ActualHeight / format.Width);
                Preview.Width = format.Height;
                Preview.Height = format.Width;
                Preview.RenderTransformOrigin = new Point(0.5, 0.5);
                Preview.RenderTransform = new ScaleTransform
                {
                    ScaleX = scale,
                    ScaleY = scale,
                };

                // Enable QR code detection
                var definition = new LumiaAnalyzerDefinition(ColorMode.Yuv420Sp, 640, AnalyzeBitmap);
                await capture.AddEffectAsync(MediaStreamType.VideoPreview, definition.ActivatableClassId, definition.Properties);

                // Start preview
                m_time.Restart();
                Preview.Source = capture;
                await capture.StartPreviewAsync();

                capture.Failed += capture_Failed;

                m_autoFocus = await ContinuousAutoFocus.StartAsync(capture.VideoDeviceController.FocusControl);

                m_capture = capture;
            }
            catch (Exception e)
            {
                TextLog.Text = String.Format("Failed to start the camera: {0}", e.Message);
            }

            m_initializing = false;
        }

        private void AnalyzeBitmap(Bitmap bitmap, TimeSpan time)
        {
            if (m_snapRequested)
            {
                m_snapRequested = false;

                IBuffer jpegBuffer = (new JpegRenderer(new BitmapImageSource(bitmap))).RenderAsync().AsTask().Result;
                var jpegFile = KnownFolders.PicturesLibrary.CreateFileAsync("QrCodeSnap.jpg", CreationCollisionOption.GenerateUniqueName).AsTask().Result;
                FileIO.WriteBufferAsync(jpegFile, jpegBuffer).AsTask().Wait();
            }

            Log.Events.QrCodeDecodeStart();

            Result result = m_reader.Decode(
                bitmap.Buffers[0].Buffer.ToArray(),
                (int)bitmap.Buffers[0].Pitch, // Should be width here but I haven't found a way to pass both width and stride to ZXing yet
                (int)bitmap.Dimensions.Height,
                BitmapFormat.Gray8
                );

            Log.Events.QrCodeDecodeStop(result != null);

            var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var elapsedTimeInMS = m_time.ElapsedMilliseconds;
                m_time.Restart();

                if (result == null)
                {
                    TextLog.Text = String.Format("[{0,4}ms] No barcode", elapsedTimeInMS);

                    if (m_autoFocus != null)
                    {
                        m_autoFocus.BarcodeFound = false;
                    }

                }
                else
                {
                    TextLog.Text = String.Format("[{0,4}ms] {1}", elapsedTimeInMS, result.Text);

                    if (m_autoFocus != null)
                    {
                        m_autoFocus.BarcodeFound = true;
                    }
                }
            });
        }

        public static async Task<string> GetBackOrDefaulCameraIdAsync()
        {
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            string deviceId = "";

            foreach (var device in devices)
            {
                if ((device.EnclosureLocation != null) &&
                    (device.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Back))
                {
                    deviceId = device.Id;
                    break;
                }
            }

            return deviceId;
        }

        // Must be called on the UI thread
        private async Task DisposeCaptureAsync()
        {
            Preview.Source = null;

            if (m_autoFocus != null)
            {
                m_autoFocus.Dispose();
                m_autoFocus = null;
            }

            MediaCapture capture;
            lock (this)
            {
                capture = m_capture;
                m_capture = null;
            }

            if (capture != null)
            {
                capture.Failed -= capture_Failed;

                await capture.StopPreviewAsync();

                capture.Dispose();
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateOverlaySize();
        }
    }
}
