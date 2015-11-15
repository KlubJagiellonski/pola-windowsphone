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
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(PolaProgressBar), new PropertyMetadata(0, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressBar b;
            PolaProgressBar progressBar = (PolaProgressBar)d;
            int value = (int)e.NewValue;
            if (value > Max)
                value = Max;
            else if (value < Min)
                value = Min;

            progressBar.ProgressBarForeground.Width = progressBar.ActualWidth * value / (Max - Min);
            progressBar.ProgressBarTextBlock.Text = string.Format("{0}%", value);
        }

        #endregion

        #endregion

        public PolaProgressBar()
        {
            this.InitializeComponent();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ProgressBarForeground.Width = ActualWidth * Value / (Max - Min);
        }
    }
}
