using Lumia.Imaging;
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
using Windows.UI.Xaml.Media.Imaging;
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

        /// <summary>
        /// Changes the visual state of the panel without animation.
        /// </summary>
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

        /// <summary>
        /// Gets or sets an object that contains all the downloaded info about the product for scanned barcode.
        /// </summary>
        public Product Product
        {
            get { return product; }
            set
            {
                product = value;

                if (product.Company != null)
                {
                    TitleTextBlock.Text = product.Company.Name;
                    PlWorkersCheck.CheckState = CheckStateFromNullableInt(product.Company.PlWorkers);
                    PlRndCheck.CheckState = CheckStateFromNullableInt(product.Company.PlRnD);
                    PlRegisteredCheck.CheckState = CheckStateFromNullableInt(product.Company.PlRegistered);
                    PlNotGlobalCheck.CheckState = CheckStateFromNullableInt(product.Company.PlNotGlobalEntity);
                }

                PlScoreBar.Value = product.PlScore;

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
            }
        }

        /// <summary>
        /// Gets a bitmap object that contains an image with the scanned barcode.
        /// </summary>
        public WriteableBitmap Bitmap { get; private set; }

        #endregion

        #region Events

        public event EventHandler<ReportEventArgs> Report;
        protected void OnReport()
        {
            if (Report != null)
                Report(this, new ReportEventArgs(product, Bitmap));
        }

        #endregion

        #region Constructor

        public ProductDetailsPanel()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Event handlers

        private void OnDismissTapped(object sender, TappedRoutedEventArgs e)
        {
            Close();
        }

        private void OnReportClick(object sender, RoutedEventArgs e)
        {
            OnReport();
        }

        #endregion

        #region Methods

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
            {
                this.Product = productItem.Product;
                this.Bitmap = productItem.Bitmap;
            }

            PlWorkersCheck.Show(TimeSpan.FromSeconds(0.2));
            PlRndCheck.Show(TimeSpan.FromSeconds(0.3));
            PlRegisteredCheck.Show(TimeSpan.FromSeconds(0.4));
            PlNotGlobalCheck.Show(TimeSpan.FromSeconds(0.5));
        }

        public void Close()
        {
            FadeOutTranslateAnimation.To = openPosition;
            FadeOutSotryboard.Begin();
            isOpen = false;
            ContentGrid.IsHitTestVisible = isOpen;
        }

        private static CheckListItemState CheckStateFromNullableInt(int? value)
        {
            if (value == null)
                return CheckListItemState.Unknown;
            if (value == 0)
                return CheckListItemState.Off;
            return CheckListItemState.On;
        }

        #endregion
    }
}
