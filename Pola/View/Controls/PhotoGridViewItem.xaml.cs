using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Pola.View.Controls
{
    public sealed partial class PhotoGridViewItem : UserControl
    {
        private const int Collumns = 4;
        private const double Margins = 38;
        private const double Spaces = 9.5;

        #region Source

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(PhotoGridViewItem), new PropertyMetadata(null, OnSourceChanged));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PhotoGridViewItem control = (PhotoGridViewItem)d;
            control.PhotoImage.Source = e.NewValue as ImageSource;
        }

        #endregion

        public PhotoGridViewItem()
        {
            this.InitializeComponent();
            this.SetupSize();
        }

        private void SetupSize()
        {
            double viewWidth = Window.Current.Bounds.Width;
            double itemWidth = (viewWidth - Margins - (Collumns - 1) * Spaces) / Collumns;
            this.Width = itemWidth;
            this.Height = itemWidth;
        }
    }
}
