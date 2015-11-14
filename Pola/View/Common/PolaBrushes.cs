using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Pola.View.Common
{
    public static class PolaBrushes
    {
        public static Brush Red 
        { 
            get { return (Brush)App.Current.Resources["PolaRedBrush"]; } 
        }

        public static Brush ProductVerifiedBackground 
        { 
            get { return (Brush)App.Current.Resources["PolaProductVerifiedBackgroundBrush"]; } 
        }

        public static Brush ProductVerifiedProgressBarBackground 
        { 
            get { return (Brush)App.Current.Resources["PolaProductVerifiedProgressBarBackgroundBrush"]; } 
        }
        
        public static Brush ProductNotVerifiedBackground 
        {
            get { return (Brush)App.Current.Resources["PolaProductNotVerifiedBackgroundBrush"]; } 
        }

        public static Brush ProductNotVerifiedProgressBarBackground
        {
            get { return (Brush)App.Current.Resources["PolaProductNotVerifiedProgressBarBackgroundBrushr"]; }
        }

        public static Brush BarcodeFrameStroke
        {
            get { return (Brush)App.Current.Resources["PolaBarcodeFrameStrokeBrush"]; }
        }

        public static Brush BarcodeFrameStrokeActive
        {
            get { return (Brush)App.Current.Resources["PolaBarcodeFrameActiveStrokeBrush"]; }
        }
    }
}
