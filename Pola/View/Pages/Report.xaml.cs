using Pola.Common;
using Pola.Model;
using Pola.Model.Json;
using Pola.View.Common;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pola.View.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Report : Page
    {
        #region Constants

        private const int PhotosCountMax = 10;

        #endregion

        #region Fields

        private NavigationHelper navigationHelper;
        private CoreApplicationView view = CoreApplication.GetCurrentView();
        private ObservableCollection<ReportPhoto> photos = new ObservableCollection<ReportPhoto>();
        private Product product;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        #endregion

        #region Constructor

        public Report()
        {
            CoreApplication.GetCurrentView();
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.view.Activated += OnViewActivated;
            this.PhotosGridView.ItemsSource = photos;
        }

        #endregion

        #region Event handlers

        private void OnViewActivated(CoreApplicationView sender, IActivatedEventArgs args1)
        {
            FileOpenPickerContinuationEventArgs args = args1 as FileOpenPickerContinuationEventArgs;

            if (args != null)
            {
                if (args.Files.Count == 0)
                    return;

                StorageFile file = args.Files[0];

                foreach (ReportPhoto photo in photos)
                    if (file.Path.Equals(photo.FilePath))
                        return;
                photos.Add(new ReportPhoto(file));
            }

            this.UpdateSendButtonAvaialbility();
            this.UpdateAddPhotoButtonAvailability();
        }

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && e.Parameter != null && e.Parameter is ReportEventArgs)
            {
                product = ((ReportEventArgs)e.Parameter).Product;
                WriteableBitmap bitmap = ((ReportEventArgs)e.Parameter).Bitmap;
                bitmap = bitmap.Rotate(90);
                photos.Add(new ReportPhoto(bitmap));
            }
            this.UpdateSendButtonAvaialbility();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private void OnAddPhotoClick(object sender, RoutedEventArgs e)
        {
            string ImagePath = string.Empty;
            FileOpenPicker filePicker = new FileOpenPicker();

            filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            filePicker.ViewMode = PickerViewMode.Thumbnail;

            filePicker.FileTypeFilter.Clear();
            filePicker.FileTypeFilter.Add(".bmp");
            filePicker.FileTypeFilter.Add(".png");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".jpg");

            filePicker.PickSingleFileAndContinue();
        }

        private void OnPhotoItemClick(object sender, ItemClickEventArgs e)
        {
            MessageDialog dialog = new MessageDialog("Czy na pewno usunąć to zdjęcie z raportu?", "Usuń zdjęcie");
            dialog.Commands.Add(new UICommand("tak", new UICommandInvokedHandler((command) =>
                {
                    photos.Remove(e.ClickedItem as ReportPhoto);
                    UpdateSendButtonAvaialbility();
                }), 0));
            dialog.Commands.Add(new UICommand("nie", null, 1));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            var ignore = dialog.ShowAsync();
        }

        private void OnCancleClick(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void OnSendClick(object sender, RoutedEventArgs e)
        {
            await SendReport();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Changes "Send" button availability depending on photos count. At least one photo is required.
        /// </summary>
        private void UpdateSendButtonAvaialbility()
        {
            SendButton.IsEnabled = photos.Count > 0;
        }

        /// <summary>
        /// Changes "Add photo" button availability depending on photos count. There is a limit of maximum photos count set to 10.
        /// </summary>
        private void UpdateAddPhotoButtonAvailability()
        {
            AddPhotoButton.IsEnabled = photos.Count < PhotosCountMax;
        }

        private async Task SendReport()
        {
            ProgressLayer.Visibility = Visibility.Visible;
            ProgressRing.IsActive = true;
            ProgressMessageTextBlock.Text = "Wysyłanie raportu";
            BottomAppBar.Visibility = Visibility.Collapsed;

            try
            {
                Model.Json.Report report = new Model.Json.Report(DescriptionTextBlock.Text, photos.Count, (product != null && product.Id != null) ? (long)product.Id : 0);
                ReportResponse reportResponse = await PolaClient.CreateReport(report);

                if (photos.Count > 0 && reportResponse.SignedRequests.Length > 0)
                {
                    int count = Math.Min(photos.Count, reportResponse.SignedRequests.Length);
                    for (int i = 0; i < count; i++)
                    {
                        var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            ProgressMessageTextBlock.Text = string.Format("Wysyłanie zdjęć {0} z {1}", i + 1, photos.Count);
                        });
                        ReportPhoto photo = photos[i];
                        string uploadUri = reportResponse.SignedRequests[i][0];

                        if (photo.Bitmap != null)
                            await PolaClient.UploadImage(uploadUri, photo.Bitmap);
                    }
                }

                ProgressRing.IsActive = false;
                ProgressMessageTextBlock.Text = string.Empty;
                if (product != null)
                    product.IsReported = true;

                MessageDialog dialog = new MessageDialog("Twoje zgłoszenie zostało wysłane i będzie rozpatrzone przez naszą redakcję.", "Pola");
                dialog.Commands.Add(new UICommand("ok") { Id = 0, });
                dialog.CancelCommandIndex = 0;
                dialog.DefaultCommandIndex = 0;
                await dialog.ShowAsync();
                Frame.GoBack();
            }
            catch
            {
                if (Frame.CurrentSourcePageType == typeof(Report))
                {
                    ProgressLayer.Visibility = Visibility.Collapsed;
                    ProgressRing.IsActive = false;

                    MessageDialog dialog = new MessageDialog("Wystąpił błąd podczas wysyłania raportu. Spróbuj ponownie później.", "Błąd");
                    var ignore = dialog.ShowAsync();
                    BottomAppBar.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

    }

    public class ReportEventArgs : EventArgs
    {
        public Product Product { get; private set; }
        public WriteableBitmap Bitmap { get; private set; }

        public ReportEventArgs(Product product, WriteableBitmap bitmap)
        {
            this.Product = product;
            this.Bitmap = bitmap;
        }
    }
}
