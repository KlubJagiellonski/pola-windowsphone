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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Pola.View.Controls
{
    public sealed partial class ProductItem : UserControl
    {
        public const double CollapsedHeight = 48;
        public const double MinimizedHeight = 30;
        public const double Space = 9.5;

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

        public string Barcode { get; set; }

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

        #endregion

        public ProductItem()
        {
            this.InitializeComponent();
            this.SetupProjection();
        }

        private void SetupProjection()
        {
            ((PlaneProjection)RootGrid.Projection).RotationX = -89.9999;
        }

        public void Expand()
        {
            Point position = this.TransformToVisual(this.GetPage()).TransformPoint(new Point());
            this.RenderTransform = new TranslateTransform()
            {
                Y = -position.Y
            };
        }

        public void Collapse()
        {

        }

        public void Minimize()
        {

        }

        public void SlideDown()
        {
            Slide(CollapsedHeight + Space);
        }

        public void SlideUp(int count = 1)
        {
            Slide(-(CollapsedHeight + Space) * count);
        }

        private void Slide(double offset)
        {
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = Position.Y,
                To = Position.Y + offset,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                EasingFunction = new QuarticEase()
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        public void Hide()
        {
            HideStoryboard.Begin();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ShowUpStoryboard.Begin();
        }

        private void OnHideStoryboardCompleted(object sender, object e)
        {
            OnHidden();
        }
    }
}
