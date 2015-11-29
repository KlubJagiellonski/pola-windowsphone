using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pola.View.Common
{
    public static class PolaConstants
    {
        public static readonly double BarcodeFrameThickness = (Double)App.Current.Resources["PolaBarcodeFrameThickness"];
        public static readonly double BarcodeFrameFilteringThickness = (Double)App.Current.Resources["PolaBarcodeFrameFilteringThickness"];
        public static readonly double BarcodeFrameActiveThickness = (Double)App.Current.Resources["PolaBarcodeFrameActiveThickness"];
        public static readonly double BarcodeFilteringOpacity = (Double)App.Current.Resources["PolaBarcodeFilteringOpacity"];
        public static readonly double BarcodeActiveOpacity = (Double)App.Current.Resources["PolaBarcodeActiveOpacity"];
    }
}
