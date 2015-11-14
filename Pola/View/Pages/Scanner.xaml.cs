using Lumia.Imaging;
using Pola.Common;
using Pola.Model;
using Pola.Model.Json;
using Pola.View.Common;
using Pola.View.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoEffects;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Phone.UI.Input;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
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
    /// A page that shows a camera preview to scan barcodes and show info about found companies.
    /// </summary>
    public sealed partial class Scanner : Page
    {
        #region Fields

        private NavigationHelper navigationHelper;

        private DisplayRequest displayRequest = new DisplayRequest();
        private MediaCapture mediaCapture;
        private ContinuousAutoFocus autoFocus;
        private bool isMediaCaptureInitializing;
        private BarcodeReader barcodeReader = new BarcodeReader
        {
            Options = new DecodingOptions
            {
                PossibleFormats = new BarcodeFormat[] { BarcodeFormat.All_1D },
                TryHarder = true
            }
        };

        private string lastBarcode = null;
        private DispatcherTimer hideBarcodeTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(1),
        };

        #endregion

        #region Properties

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        #endregion

        #region Constructors

        public Scanner()
        {
            this.InitializeComponent();
            this.SetupApplicatoinBar();
            this.SetupBarcodeTimer();

            this.navigationHelper = new NavigationHelper(this);
            HardwareButtons.BackPressed += OnBackPressed;
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            this.SetupStatusBar();

            // Prevent screen timeout
            displayRequest.RequestActive();

            Application.Current.Resuming += OnResuming;
            Application.Current.Suspending += OnSuspending;
            Window.Current.VisibilityChanged += OnWindowVisibilityChanged;

            var ignore = InitializeCaptureAsync();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);

            Application.Current.Resuming -= OnResuming;
            Application.Current.Suspending -= OnSuspending;
            Window.Current.VisibilityChanged -= OnWindowVisibilityChanged;

            displayRequest.RequestRelease();

            await DisposeCaptureAsync();
        }

        private void OnBackPressed(object sender, BackPressedEventArgs e)
        {
            if (ProductDetailsPanel.IsOpen)
            {
                e.Handled = true;
                ProductDetailsPanel.Close();
            }
        }

        private void OnResuming(object sender, object e)
        {
            var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await InitializeCaptureAsync();
            });
        }

        private void OnSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await DisposeCaptureAsync();
            });
        }

        private async void OnWindowVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            if (e.Visible)
                await InitializeCaptureAsync();
            else
                await DisposeCaptureAsync();
        }

        private void OnMediaCaptureFailed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await DisposeCaptureAsync();
            });
        }

        private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateOverlaySize();
        }

        private void OnRateClick(object sender, RoutedEventArgs e)
        {

        }

        private void OnFeedbackClick(object sender, RoutedEventArgs e)
        {

        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(About));
        }

        private void OnProductSelected(object sender, ProductEventArgs e)
        {
            ProductDetailsPanel.Open((ProductItem)sender);
        }

        private void OnHideBarcodeTimerTick(object sender, object e)
        {
            var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    BarcodeTextBlock.Opacity = 0;
                    BarcodeFrame.Stroke = PolaBrushes.BarcodeFrameStroke;
                    BarcodeFrame.StrokeThickness = PolaConstants.BarcodeFrameThickness;
                });
        }

        #endregion

        #region Methods

        public void SetupApplicatoinBar()
        {
            this.BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
        }

        public void SetupBarcodeTimer()
        {
            hideBarcodeTimer.Tick += OnHideBarcodeTimerTick;
        }

        /// <summary>
        /// Setups StatusBar opacity and colors.
        /// </summary>
        public void SetupStatusBar()
        {
            StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            statusBar.BackgroundOpacity = 0;
            statusBar.ForegroundColor = Colors.White;
        }

        /// <summary>
        /// Updates overlay size to make it fullscreen and keep proportions.
        /// </summary>
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

        private async Task InitializeCaptureAsync()
        {
            if (isMediaCaptureInitializing || (mediaCapture != null))
            {
                return;
            }
            isMediaCaptureInitializing = true;

            try
            {
                var settings = new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = await GetBackOrDefaulCameraIdAsync(),
                    StreamingCaptureMode = StreamingCaptureMode.Video
                };

                var newMediaCapture = new MediaCapture();
                await newMediaCapture.InitializeAsync(settings);
                newMediaCapture.SetPreviewRotation(VideoRotation.Clockwise90Degrees);

                // Select the capture resolution closest to screen resolution
                var formats = newMediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);
                var format = (VideoEncodingProperties)formats.OrderBy((item) =>
                {
                    var props = (VideoEncodingProperties)item;
                    return Math.Abs(props.Width - this.ActualHeight) + Math.Abs(props.Height - this.ActualWidth);
                }).First();

                Debug.WriteLine("{0} x {1}", format.Width, format.Height);

                await newMediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, format);

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
                await newMediaCapture.AddEffectAsync(MediaStreamType.VideoPreview, definition.ActivatableClassId, definition.Properties);

                // Start preview
                Preview.Source = newMediaCapture;
                await newMediaCapture.StartPreviewAsync();

                newMediaCapture.Failed += OnMediaCaptureFailed;

                autoFocus = await ContinuousAutoFocus.StartAsync(newMediaCapture.VideoDeviceController.FocusControl);

                mediaCapture = newMediaCapture;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to start the camera: {0}", e.Message);
            }

            isMediaCaptureInitializing = false;
        }

        private void AnalyzeBitmap(Bitmap bitmap, TimeSpan time)
        {
            Log.Events.QrCodeDecodeStart();

            Result result = barcodeReader.Decode(
                bitmap.Buffers[0].Buffer.ToArray(),
                (int)bitmap.Buffers[0].Pitch, // Should be width here but I haven't found a way to pass both width and stride to ZXing yet
                (int)bitmap.Dimensions.Height,
                BitmapFormat.Gray8
                );

            Log.Events.QrCodeDecodeStop(result != null);

            var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (result != null && IsValidEan(result.Text))
                {
                    if (autoFocus != null)
                        autoFocus.BarcodeFound = true;

                    string barcode = result.Text;
                    BarcodeTextBlock.Text = barcode.ToEanString();
                    BarcodeTextBlock.Opacity = 1;
                    BarcodeFrame.Stroke = PolaBrushes.BarcodeFrameStrokeActive;
                    BarcodeFrame.StrokeThickness = PolaConstants.BarcodeFrameActiveThickness;
                    hideBarcodeTimer.Start();

                    if (lastBarcode != barcode)
                    {
                        Debug.WriteLine(barcode);
                        lastBarcode = barcode;
                        ProductsListBox.AddProduct(barcode);
                    }
                }
                else
                {
                    if (autoFocus != null)
                        autoFocus.BarcodeFound = false;
                }
            });
        }

        private static Regex eanRegex = new System.Text.RegularExpressions.Regex("^(\\d{8}|\\d{12,14})$");
        public static bool IsValidEan(string code)
        {
            if (!(eanRegex.IsMatch(code))) return false; // Check if all digits and with 8, 12, 13 or 14 digits.
            string extendedCode = code.PadLeft(14, '0'); // Stuff zeros at start to garantee 14 digits.
            int[] mult = Enumerable.Range(0, 13).Select(i => ((int)(extendedCode[i] - '0')) * ((i % 2 == 0) ? 3 : 1)).ToArray(); // STEP 1: Without check digit, "Multiply value of each position" by 3 or 1.
            int sum = mult.Sum(); // STEP 2: "Add results together to create sum".
            return (10 - (sum % 10)) % 10 == int.Parse(extendedCode[13].ToString()); // STEP 3: Equivalent to "Subtract the sum from the nearest equal or higher multiple of ten = CHECK DIGIT".
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

            if (autoFocus != null)
            {
                autoFocus.Dispose();
                autoFocus = null;
            }

            MediaCapture mediaCapture;
            lock (this)
            {
                mediaCapture = this.mediaCapture;
                this.mediaCapture = null;
            }

            if (mediaCapture != null)
            {
                mediaCapture.Failed -= OnMediaCaptureFailed;

                await mediaCapture.StopPreviewAsync();

                mediaCapture.Dispose();
            }
        }

        #endregion
    }
}
