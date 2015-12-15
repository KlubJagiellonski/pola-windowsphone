using Pola.Model;
using Pola.Model.Json;
using Pola.View.Common;
using System;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Pola.View.Controls
{
    /// <summary>
    /// Shows the name of the company that has produced a particular product and its PL score. 
    /// This control is used as an item on a list of products found by barcode. 
    /// </summary>
    public sealed partial class ProductItem : UserControl
    {
        #region Constants

        public static readonly double DefaultHeight = (Double)App.Current.Resources["PolaProductItemHeight"];
        public static readonly double Space = (Double)App.Current.Resources["PolaProductItemSpace"];

        #endregion

        #region Fields

        private string barcode;
        private WriteableBitmap bitmap;
        private Product product;
        private double? targetY = null;

        #endregion

        #region Events

        public event EventHandler Hidden;
        protected void OnHidden()
        {
            if (Hidden != null)
                Hidden(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets barcode of product.
        /// </summary>
        public string Barcode
        {
            get
            {
                return barcode;
            }
        }

        /// <summary>
        /// Sets on gets translation of the control based on its transform matrix.
        /// </summary>
        public Point Translation
        {
            get
            {
                return new Point(((CompositeTransform)RenderTransform).TranslateX, ((CompositeTransform)RenderTransform).TranslateY);
            }

            set
            {
                ((CompositeTransform)RenderTransform).TranslateX = value.X;
                ((CompositeTransform)RenderTransform).TranslateY = value.Y;
            }
        }

        /// <summary>
        /// Gets product found by its barcode.
        /// </summary>
        public Product Product
        {
            get
            {
                return product;
            }
        }

        /// <summary>
        /// Gets a bitmap object that contains an image with the scanned barcode.
        /// </summary>
        public WriteableBitmap Bitmap
        {
            get
            {
                return bitmap;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new product item. Once it's created it starts downloading product info.
        /// </summary>
        /// <param name="barcode">Barcode of product.</param>
        public ProductItem(string barcode, WriteableBitmap bitmap)
        {
            this.InitializeComponent();
            this.SetupProjection();
            this.barcode = barcode;
            this.bitmap = bitmap;
            FindProduct();
        }

        #endregion

        #region Event handlers

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ShowUpStoryboard.Begin();
        }

        private void OnHideStoryboardCompleted(object sender, object e)
        {
            OnHidden();
        }

        #endregion

        #region Methods

        private async void FindProduct()
        {
            product = await PolaClient.FindProduct(barcode);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ProgressBar.IsIndeterminate = false;

                if (product == null)
                {
                    TitleTextBlock.Text = "Brak informacji";
                    return;
                }

                if (product.PlScore != null)
                    ProgressBar.Value = (int)product.PlScore;


                TitleTextBlock.Text = product.Name;
                TitleTextBlock.Opacity = 1;
                //TitleTextBlock.Text = "Nieznany produkt (dotknij, aby zgłosić)";
                //TitleTextBlock.Text = "Brak informacji";

                switch (product.CardType)
                {
                    case CardType.White:
                        RootGrid.Background = PolaBrushes.ProductVerifiedBackground;
                        ProgressBar.Background = PolaBrushes.ProductVerifiedProgressBarBackground;
                        break;
                    case CardType.Grey:
                        RootGrid.Background = PolaBrushes.ProductNotVerifiedBackground;
                        ProgressBar.Background = PolaBrushes.ProductNotVerifiedProgressBarBackground;
                        break;
                }
            });
        }

        private void SetupProjection()
        {
            ((PlaneProjection)RootGrid.Projection).RotationX = -89.9999;
        }

        public void SlideDown()
        {
            Slide(DefaultHeight + Space);
        }

        public void SlideUp(int count = 1)
        {
            Slide(-(DefaultHeight + Space) * count);
        }

        private void Slide(double offset)
        {
            if (targetY == null)
                targetY = Translation.Y;
            targetY += offset;

            Storyboard storyboard = new Storyboard();

            DoubleAnimation translateAnimation = new DoubleAnimation()
            {
                To = targetY,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                EasingFunction = new QuarticEase()
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };
            Storyboard.SetTarget(translateAnimation, this);
            Storyboard.SetTargetProperty(translateAnimation, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            storyboard.Children.Add(translateAnimation);

            if (offset < 0)
            {
                DoubleAnimation scaleXAnimation = new DoubleAnimation()
                {
                    From = 1,
                    To = 1.05,
                    Duration = new Duration(TimeSpan.FromSeconds(0.25)),
                    AutoReverse = true,
                    EasingFunction = new QuarticEase()
                    {
                        EasingMode = EasingMode.EaseOut
                    }
                };
                Storyboard.SetTarget(scaleXAnimation, this);
                Storyboard.SetTargetProperty(scaleXAnimation, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
                storyboard.Children.Add(scaleXAnimation);

                DoubleAnimation scaleYAnimation = new DoubleAnimation()
                {
                    From = 1,
                    To = 1.05,
                    Duration = new Duration(TimeSpan.FromSeconds(0.25)),
                    AutoReverse = true,
                    EasingFunction = new QuarticEase()
                    {
                        EasingMode = EasingMode.EaseOut
                    }
                };
                Storyboard.SetTarget(scaleYAnimation, this);
                Storyboard.SetTargetProperty(scaleYAnimation, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
                storyboard.Children.Add(scaleYAnimation);
            }
            storyboard.Begin();
        }

        public void Hide()
        {
            HideStoryboard.Begin();
        }

        #endregion
    }
}
