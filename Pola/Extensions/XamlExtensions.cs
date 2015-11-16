using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows.UI.Xaml
{
    public static class XamlExtensions
    {
        public static bool IsVisible(this UIElement element)
        {
            return element.Visibility == Visibility.Visible;
        }

        public static void IsVisible(this UIElement element, bool isVisible)
        {
            element.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
