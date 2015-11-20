using Lumia.Imaging;
using Pola.Model.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Pola.View.Common
{
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
