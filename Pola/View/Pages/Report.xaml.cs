using Lumia.Imaging;
using Pola.Common;
using Pola.Model;
using Pola.Model.Json;
using Pola.View.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
        private NavigationHelper navigationHelper;
        private CoreApplicationView view = CoreApplication.GetCurrentView();
        private ObservableCollection<ReportPhoto> photos = new ObservableCollection<ReportPhoto>();
        private int productId;

        public Report()
        {
            CoreApplication.GetCurrentView();
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.view.Activated += OnViewActivated;
            this.PhotosGridView.ItemsSource = photos;
        }

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
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        #region NavigationHelper registration

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
                Product product = ((ReportEventArgs)e.Parameter).Product;
                productId = product.Id;
                WriteableBitmap bitmap = ((ReportEventArgs)e.Parameter).Bitmap;
                bitmap = bitmap.Rotate(90);
                photos.Add(new ReportPhoto(bitmap));
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
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

        private void OnCancleClick(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void OnSendClick(object sender, RoutedEventArgs e)
        {
            Model.Json.Report report = new Model.Json.Report(DescriptionTextBlock.Text, photos.Count, productId);
            ReportResponse reportResponse = await PolaClient.CreateReport(report);
            if (photos.Count > 0 && reportResponse.SignedRequests.Length > 0)
            {
                int count = Math.Min(photos.Count, reportResponse.SignedRequests.Length);
                for (int i = 0; i < count; i++)
                {
                    ReportPhoto photo = photos[i];
                    string uploadUri = reportResponse.SignedRequests[i][0];

                    if (photo.Bitmap != null)
                        await PolaClient.UploadImage(uploadUri, photo.Bitmap);
                }
            }
        }
    }
}
