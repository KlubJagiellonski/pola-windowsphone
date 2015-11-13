using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Windows.UI.Xaml
{
    public static class XamlExtensions
    {
        public static Page GetPage(this FrameworkElement element)
        {
            DependencyObject parent = element.Parent;
            while (parent != null && parent is FrameworkElement)
            {
                if (parent is Page)
                    return (Page)parent;
                parent = ((FrameworkElement)parent).Parent;
            }
            return null;
        }
    }
}
