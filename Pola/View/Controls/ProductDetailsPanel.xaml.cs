using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Pola.View.Controls
{
    public sealed partial class ProductDetailsPanel : UserControl
    {
        private bool isOpen;

        public bool IsOpen
        {
            get
            {
                return isOpen;
            }

            set
            {
                isOpen = value;
                if (isOpen)
                {
                    ContentGrid.Opacity = 1;
                    ((TranslateTransform)ContentGrid.RenderTransform).Y = 0;
                    DismissLayer.Visibility = Visibility.Visible;
                    DismissLayer.Opacity = 0.8;
                }
                else
                {
                    ContentGrid.Opacity = 0;
                    ((TranslateTransform)ContentGrid.RenderTransform).Y = 400;
                    DismissLayer.Visibility = Visibility.Collapsed;
                    DismissLayer.Opacity = 0;
                }
                ContentGrid.IsHitTestVisible = isOpen;
            }
        }

        public ProductDetailsPanel()
        {
            this.InitializeComponent();
        }

        public void Open()
        {
            FadeInSotryboard.Begin();
            isOpen = true;
            ContentGrid.IsHitTestVisible = isOpen;
        }

        public void Close()
        {
            FadeOutSotryboard.Begin();
            isOpen = false;
            ContentGrid.IsHitTestVisible = isOpen;
        }

        private void DismissLayer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Close();
        }
    }
}
