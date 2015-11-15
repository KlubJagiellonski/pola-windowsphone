using Pola.Model.Json;
using Pola.View.Common;
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
        #region Fields

        private bool isOpen;
        private double openPosition = 400;
        private Product product;

        #endregion

        #region Properties

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
                    ((CompositeTransform)ContentGrid.RenderTransform).TranslateY = 0;
                    ((CompositeTransform)ContentGrid.RenderTransform).ScaleX = 1;
                    ((CompositeTransform)ContentGrid.RenderTransform).ScaleY = 1;
                    DismissLayer.Visibility = Visibility.Visible;
                    DismissLayer.Opacity = 0.8;
                }
                else
                {
                    ContentGrid.Opacity = 0;
                    ((CompositeTransform)ContentGrid.RenderTransform).TranslateY = openPosition;
                    ((CompositeTransform)ContentGrid.RenderTransform).ScaleX = 0.95;
                    ((CompositeTransform)ContentGrid.RenderTransform).ScaleY = 0.75;
                    DismissLayer.Visibility = Visibility.Collapsed;
                    DismissLayer.Opacity = 0;
                }
                ContentGrid.IsHitTestVisible = isOpen;
            }
        }

        public Product Prodcut
        {
            get { return product; }
            set
            {
                product = value;

                if (product.Company != null)
                {
                    TitleTextBlock.Text = product.Company.Name;
                    PlWorkersCheck.IsChecked = product.Company.PlWorkers > 0;
                    PlRndCheck.IsChecked = product.Company.PlRnD > 0;
                    PlRegisteredCheck.IsChecked = product.Company.PlRegistered > 0;
                    PlNotGlobalCheck.IsChecked = product.Company.PlNotGlobalEntity > 0;
                }

                if (product.PlScore != null)
                    PlScoreBar.Value = (int)product.PlScore;
                else
                    PlScoreBar.Value = 0;

                if (product.Company != null && product.Company.PlCapital != null)
                    PlCapitalBar.Value = (int)product.Company.PlCapital;
                else
                    PlCapitalBar.Value = 0;

                if (product.IsVerified)
                {
                    ContentGrid.Background = PolaBrushes.ProductVerifiedBackground;
                    PlScoreBar.Background = PolaBrushes.ProductVerifiedProgressBarBackground;
                }
                else
                {
                    ContentGrid.Background = PolaBrushes.ProductNotVerifiedBackground;
                    PlScoreBar.Background = PolaBrushes.ProductNotVerifiedProgressBarBackground;
                }

                ReportButton.Visibility = product.NeedsReport ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region Constructor

        public ProductDetailsPanel()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Event handlers

        private void DismissLayer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Methods

        public void Open()
        {
            FadeInSotryboard.Begin();
            isOpen = true;
            ContentGrid.IsHitTestVisible = isOpen;
        }

        public void Open(ProductItem productItem)
        {
            GeneralTransform productItemTransform = this.TransformToVisual(productItem);
            Point productItemPosition = productItemTransform.TransformPoint(new Point(0, ContentGrid.ActualHeight / 2));
            openPosition = -productItemPosition.Y + 19; // Measured offset just better looking animation.
            ((CompositeTransform)ContentGrid.RenderTransform).TranslateY = openPosition;
            FadeInSotryboard.Begin();
            isOpen = true;
            ContentGrid.IsHitTestVisible = isOpen;
            if (productItem.Product != null)
                this.Prodcut = productItem.Product;
        }

        public void Close()
        {
            FadeOutTranslateAnimation.To = openPosition;
            FadeOutSotryboard.Begin();
            isOpen = false;
            ContentGrid.IsHitTestVisible = isOpen;
        }

        #endregion
    }
}
