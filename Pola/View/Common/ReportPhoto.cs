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
        public WriteableBitmap Bitmap { get; private set; }
        public string FilePath { get; private set; }

        public ReportPhoto(WriteableBitmap bitmap)
        {
            ThumbnailSource = bitmap;
            Bitmap = bitmap;
        }

        public ReportPhoto(StorageFile file)
        {
            FilePath = file.Path;

            StorageItemThumbnail thumbnail = file.GetThumbnailAsync(ThumbnailMode.ListView).AsTask().Result;
            BitmapImage thumbnailImage = new BitmapImage();
            var ignore = thumbnailImage.SetSourceAsync(thumbnail);
            ThumbnailSource = thumbnailImage;

            StorageItemThumbnail imageAsThumbnail = file.GetScaledImageAsThumbnailAsync(ThumbnailMode.SingleItem).AsTask().Result;
            WriteableBitmap bitmap = new WriteableBitmap((int)imageAsThumbnail.OriginalWidth, (int)imageAsThumbnail.OriginalHeight);
            ignore = bitmap.SetSourceAsync(imageAsThumbnail);
            Bitmap = bitmap;
        }
    }
}
