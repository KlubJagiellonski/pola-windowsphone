using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Pola.View.Common
{
    public class ReportPhoto
    {
        public ImageSource ThumbnailSource { get; private set; }
        public StorageFile PhotoFile { get; private set; }

        public ReportPhoto(WriteableBitmap bitmap)
        {
            ThumbnailSource = bitmap;
        }

        public ReportPhoto(StorageFile file)
        {
            StorageItemThumbnail thumbnail = file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView).AsTask().Result;
            BitmapImage thumbnailImage = new BitmapImage();
            var ignore = thumbnailImage.SetSourceAsync(thumbnail);
            ThumbnailSource = thumbnailImage;
        }
    }
}
