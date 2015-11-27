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
    /// <summary>
    /// Simple progress bar designed for % values. The value is also presented as a text.
    /// </summary>
    public sealed partial class PolaProgressBar : UserControl
    {
        #region Constatns

        private const int Min = 0;
        private const int Max = 100;

        #endregion

        #region Properties

        #region Value

        /// <summary>
        /// Gets or sets current setting of the range control in %.
        /// </summary>
        public int? Value
        {
            get { return (int?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(PolaProgressBar), new PropertyMetadata(0, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PolaProgressBar progressBar = (PolaProgressBar)d;
            if (e.NewValue == null)
            {
                progressBar.ProgressBarForeground.Width = 0;
                progressBar.ProgressBarTextBlock.Text = string.Empty;
            }
            else
            {
                int value = (int)e.NewValue;
                if (value > Max)
                    value = Max;
                else if (value < Min)
                    value = Min;

                progressBar.ProgressBarForeground.Width = progressBar.ActualWidth * value / (Max - Min);
                progressBar.ProgressBarTextBlock.Text = string.Format("{0}%", value);

                progressBar.UpdateLayout();

                if (progressBar.ProgressBarTextBlock.ActualWidth + 19 > progressBar.ActualWidth - progressBar.ProgressBarForeground.ActualWidth)
                {
                    progressBar.ProgressBarTextBlock.Margin = new Thickness(0, -2.5, progressBar.ActualWidth - progressBar.ProgressBarForeground.ActualWidth + 9.5, 0);
                    progressBar.ProgressBarTextBlock.Foreground = progressBar.ForegroundTextBrush;
                }
                else
                {
                    progressBar.ProgressBarTextBlock.Margin = new Thickness(0, -2.5, 9.5, 0);
                    progressBar.ProgressBarTextBlock.Foreground = progressBar.BackgroundTextBrush;
                }
            }
        }

        #endregion

        #region ForegroundTextBrush

        public Brush ForegroundTextBrush
        {
            get { return (Brush)GetValue(ForegroundTextBrushProperty); }
            set { SetValue(ForegroundTextBrushProperty, value); }
        }

        public static readonly DependencyProperty ForegroundTextBrushProperty =
            DependencyProperty.Register("ForegroundTextBrush", typeof(Brush), typeof(PolaProgressBar), new PropertyMetadata(PolaBrushes.White));

        #endregion

        #region BackgroundTextBrush

        public Brush BackgroundTextBrush
        {
            get { return (Brush)GetValue(BackgroundTextBrushProperty); }
            set { SetValue(BackgroundTextBrushProperty, value); }
        }

        public static readonly DependencyProperty BackgroundTextBrushProperty =
            DependencyProperty.Register("BackgroundTextBrush", typeof(Brush), typeof(PolaProgressBar), new PropertyMetadata(PolaBrushes.Black));

        #endregion

        #endregion

        public PolaProgressBar()
        {
            this.InitializeComponent();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            int value = Value ?? 0;
            ProgressBarForeground.Width = ActualWidth * value / (Max - Min);
        }
    }
}
