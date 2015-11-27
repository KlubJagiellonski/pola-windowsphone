using Lumia.Imaging;
using Pola.Common;
using Pola.View.Common;
using Pola.View.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoEffects;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Phone.UI.Input;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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
                PossibleFormats = new BarcodeFormat[] { BarcodeFormat.EAN_8, BarcodeFormat.EAN_13 },
                TryHarder = true
            }
        };

        private string lastBarcode = null;
        private DispatcherTimer hideBarcodeTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        private BarcodeFilter barcodeFilter = new BarcodeFilter();

        private WriteableBitmap bitmapWithBarcode;

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
            this.SetupBarcodeFilter();

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

            if (ProductDetailsPanel.IsOpen)
                ProductDetailsPanel.Close();

            ProductsListBox.RemoveReportedProducts();
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
            var ignore = Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=1b189a4d-2f48-4f99-a3be-72623e7f711f"));
        }

        private void OnFeedbackClick(object sender, RoutedEventArgs e)
        {
            string version = Package.Current.Id.Version.ToVersion().ToString();
            string email = "kontakt@pola-app.pl";
            string subject = string.Format("Pola {0}, Windows Phone", version);
            string body = "";
            Uri mailto = new Uri(string.Format("mailto:?to={0}&subject={1}&body={2}", email, subject, body));
            var ignore = Launcher.LaunchUriAsync(mailto);
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(About));
        }

        private void OnProductSelected(object sender, ProductEventArgs e)
        {
            ProductItem productItem = (ProductItem)sender;
            if (productItem.Product.Company != null)
                ProductDetailsPanel.Open(productItem);
            else
                Frame.Navigate(typeof(Report), new ReportEventArgs(productItem.Product, productItem.Bitmap));
        }

        private void OnProductReport(object sender, ReportEventArgs e)
        {
            Frame.Navigate(typeof(Report), e);
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

        private void OnAppBarClosed(object sender, object e)
        {
            this.BottomAppBar.Opacity = 0;
        }

        private void OnAppBarOpened(object sender, object e)
        {
            this.BottomAppBar.Opacity = 1;
        }

        private void OnNewBarcodeDetected(object sender, BarcodeEventArgs e)
        {
            string barcode = e.Barcode;
            if (lastBarcode != barcode)
            {
                Debug.WriteLine(barcode);
                lastBarcode = barcode;

                var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ProductsListBox.AddProduct(barcode, bitmapWithBarcode);
                    HintTextBlock.Visibility = Visibility.Collapsed;
                    bitmapWithBarcode = new WriteableBitmap(bitmapWithBarcode.PixelWidth, bitmapWithBarcode.PixelHeight);
                });
            }
        }

        #endregion

        #region Methods

        private void SetupApplicatoinBar()
        {
            this.BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
        }

        private void SetupBarcodeTimer()
        {
            hideBarcodeTimer.Tick += OnHideBarcodeTimerTick;
        }

        private void SetupBarcodeFilter()
        {
            barcodeFilter.MinPass = 3;
            barcodeFilter.FailsThreshold = 2;
            barcodeFilter.NewBarcodeDetected += OnNewBarcodeDetected;
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

                // Select the capture resolution closest to screen resolution
                var formats = newMediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);
                var format = (VideoEncodingProperties)formats.OrderBy((item) =>
                {
                    var props = (VideoEncodingProperties)item;
                    return Math.Abs(props.Width - this.ActualHeight) + Math.Abs(props.Height - this.ActualWidth);
                }).First();

                Debug.WriteLine("{0} x {1}", format.Width, format.Height);

                await newMediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, format);

                newMediaCapture.VideoDeviceController.FlashControl.Enabled = false;

                // Prepare bitmap for reports
                bitmapWithBarcode = new WriteableBitmap((int)format.Width, (int)format.Height);

                // Make the preview full screen
                Preview.Width = this.ActualHeight;
                Preview.Height = this.ActualWidth;

                // Enable QR code detection
                var definition = new LumiaAnalyzerDefinition(ColorMode.Yuv420Sp, Math.Min(format.Width, 800), AnalyzeBitmap);
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
            if (ProductDetailsPanel.IsOpen)
                return;

            // Miejsce na wycięcie piskeli

            Result result = barcodeReader.Decode(
                bitmap.Buffers[0].Buffer.ToArray(),
                (int)bitmap.Buffers[0].Pitch, // Should be width here but I haven't found a way to pass both width and stride to ZXing yet
                (int)bitmap.Dimensions.Height,
                BitmapFormat.Gray8);

            if (result != null && IsValidEan(result.Text))
            {
                if (autoFocus != null)
                    autoFocus.BarcodeFound = true;

                string barcode = result.Text;

                var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
                {
                    BarcodeTextBlock.Text = barcode.ToEanString();
                    BarcodeTextBlock.Opacity = 1;
                    BarcodeFrame.Stroke = PolaBrushes.BarcodeFrameStrokeActive;
                    BarcodeFrame.StrokeThickness = PolaConstants.BarcodeFrameActiveThickness;
                    hideBarcodeTimer.Start();
                });

                BitmapImageSource bmpImgSrc = new BitmapImageSource(bitmap);
                WriteableBitmapRenderer renderer = new WriteableBitmapRenderer(bmpImgSrc, bitmapWithBarcode);
                bitmapWithBarcode = renderer.RenderAsync().AsTask().Result;

                barcodeFilter.Update(barcode);
            }
            else
            {
                if (autoFocus != null)
                    autoFocus.BarcodeFound = false;
            }

        }

        private static Regex eanRegex = new System.Text.RegularExpressions.Regex("^(\\d{8}|\\d{12,14})$");
        public static bool IsValidEan(string code)
        {
            if (!(eanRegex.IsMatch(code))) return false; // Check if all digits and with 8, 12 or 13 digits.
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
