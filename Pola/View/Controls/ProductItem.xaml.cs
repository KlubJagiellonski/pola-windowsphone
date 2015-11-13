using Pola.Model;
using Pola.Model.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Pola.View.Controls
{
    public sealed partial class ProductItem : UserControl
    {
        #region Constants

        public const double DefaultHeight = 48;
        public const double Space = 9.5;

        #endregion

        #region Fields

        private string barcode;
        private Product product;

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

        #region Title

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ProductItem), new PropertyMetadata(0, OnTitleChanged));

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProductItem panel = (ProductItem)d;
            panel.TitleTextBlock.Text = (string)e.NewValue;
        }

        #endregion

        public string Barcode
        {
            get
            {
                return barcode;
            }
        }

        public Point Position
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

        public Product Product
        {
            get
            {
                return product;
            }
        }

        #endregion

        #region Constructor

        public ProductItem(string barcode)
        {
            this.InitializeComponent();
            this.SetupProjection();
            this.barcode = barcode;
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
                if (product.PlScore != null)
                    ProgressBar.Value = (int)product.PlScore;

                if (product.Company != null && product.Company.Name != null)
                {
                    TitleTextBlock.Text = product.Company.Name;
                    TitleTextBlock.Opacity = 1;
                }
                else
                    TitleTextBlock.Text = "Nieznany produkt";
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
            Storyboard storyboard = new Storyboard();

            DoubleAnimation translateAnimation = new DoubleAnimation()
            {
                From = Position.Y,
                To = Position.Y + offset,
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
