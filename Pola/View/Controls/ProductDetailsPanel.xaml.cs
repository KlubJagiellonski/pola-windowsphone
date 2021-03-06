﻿using Pola.Model.Json;
using Pola.View.Common;
using Pola.View.Pages;
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Pola.View.Controls
{
    public sealed partial class ProductDetailsPanel : UserControl
    {
        #region Constants

        const int LessLines = 3;
        const int MoreLines = 0;

        const string ShowMoreText = "pokaż więcej";
        const string ShowLessText = "pokaż mniej";

        #endregion

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

                TitleTextBlock.Text = product.Name;
                if (product.AltText != null)
                {
                    AltTextBlock.Visibility = Visibility.Visible;
                    ProductDescription.Visibility = Visibility.Collapsed;
                    AltTextBlock.Text = product.AltText;

                    PlWorkersCheck.CheckState = CheckListItemState.Unknown;
                    PlRndCheck.CheckState = CheckListItemState.Unknown;
                    PlRegisteredCheck.CheckState = CheckListItemState.Unknown;
                    PlNotGlobalCheck.CheckState = CheckListItemState.Unknown;
                    PlCapitalBar.Value = null;
                    PlScoreBar.Value = null;
                    DescriptionTextBlock.Text = string.Empty;
                }
                else
                {
                    AltTextBlock.Visibility = Visibility.Collapsed;
                    ProductDescription.Visibility = Visibility.Visible;
                    AltTextBlock.Text = string.Empty;

                    PlWorkersCheck.CheckState = CheckStateFromNullableInt(product.PlWorkers);
                    PlRndCheck.CheckState = CheckStateFromNullableInt(product.PlRnD);
                    PlRegisteredCheck.CheckState = CheckStateFromNullableInt(product.PlRegistered);
                    PlNotGlobalCheck.CheckState = CheckStateFromNullableInt(product.PlNotGlobalEntity);
                    PlCapitalBar.Value = product.PlCapital;
                    PlScoreBar.Value = product.PlScore;

                    // Set description text
                    DescriptionTextBlock.Text = product.Description;

                    // Allow to expand
                    DescriptionTextBlock.MaxLines = MoreLines;
                    
                    // Update size
                    DescriptionTextBlock.UpdateLayout();

                    // Save the height
                    double maximumHeight = DescriptionTextBlock.DesiredSize.Height;//.ActualHeight;

                    // Limit the number of lines
                    DescriptionTextBlock.MaxLines = LessLines;

                    // Update size
                    DescriptionTextBlock.UpdateLayout();

                    // Seve the new hegiht
                    double minimumHeight = DescriptionTextBlock.DesiredSize.Height;

                    // Hide "show more" link if the description is not longer than 3 lines
                    ShowMoreLink.Visibility = maximumHeight > minimumHeight ? Visibility.Visible : Visibility.Collapsed;
                    ShowMoreTextBlock.Text = ShowMoreText;
                }

                switch (product.CardType)
                {
                    case CardType.White:
                        ContentGrid.Background = PolaBrushes.ProductVerifiedBackground;
                        PlScoreBar.Background = PolaBrushes.ProductVerifiedProgressBarBackground;
                        break;
                    case CardType.Grey:
                        ContentGrid.Background = PolaBrushes.ProductNotVerifiedBackground;
                        PlScoreBar.Background = PolaBrushes.ProductNotVerifiedProgressBarBackground;
                        break;
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
        private void OnReport()
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

        private void OnMoreClick(object sender, TappedRoutedEventArgs e)
        {
            if (DescriptionTextBlock.MaxLines == LessLines)
            {
                DescriptionTextBlock.MaxLines = MoreLines;
                ShowMoreTextBlock.Text = ShowLessText;
            }
            else
            {
                DescriptionTextBlock.MaxLines = LessLines;
                ShowMoreTextBlock.Text = ShowMoreText;
            }
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
