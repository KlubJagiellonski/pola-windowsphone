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
    public enum CheckListItemState
    {
        On,
        Off,
        Unknown,
    }

    public sealed partial class CheckListItem : UserControl
    {
        /// <summary>
        /// Gets or sets whether the check is checked.
        /// </summary>
        public CheckListItemState CheckState
        {
            get
            {
                if (CheckOn.IsVisible())
                    return CheckListItemState.On;
                if (CheckOff.IsVisible())
                    return CheckListItemState.Off;
                return CheckListItemState.Unknown;
            }

            set
            {
                if (value == CheckListItemState.On)
                {
                    CheckOn.Visibility = Visibility.Visible;
                    CheckOff.Visibility = Visibility.Collapsed;
                    CheckUnknown.Visibility = Visibility.Collapsed;
                    CheckBacground.Fill = PolaBrushes.Red;
                }
                else if (value == CheckListItemState.Off)
                {
                    CheckOn.Visibility = Visibility.Collapsed;
                    CheckOff.Visibility = Visibility.Visible;
                    CheckUnknown.Visibility = Visibility.Collapsed;
                    CheckBacground.Fill = PolaBrushes.ProductVerifiedProgressBarBackground;
                }
                else
                {
                    CheckOn.Visibility = Visibility.Collapsed;
                    CheckOff.Visibility = Visibility.Collapsed;
                    CheckUnknown.Visibility = Visibility.Visible;
                    CheckBacground.Fill = PolaBrushes.ProductVerifiedProgressBarBackground;
                }
            }
        }

        /// <summary>
        /// Gets or sets the text next to the check mark.
        /// </summary>
        public string Title
        {
            get
            {
                return TitleTextBlock.Text;
            }

            set
            {
                TitleTextBlock.Text = value;
            }
        }

        public CheckListItem()
        {
            this.InitializeComponent();
        }

        public void Show(TimeSpan delay)
        {
            ShowStoryboard.BeginTime = delay;
            ((ScaleTransform)CheckBacground.RenderTransform).ScaleX = 0;
            ((ScaleTransform)CheckBacground.RenderTransform).ScaleY = 0;
            ((ScaleTransform)CheckSymbol.RenderTransform).ScaleX = 0;
            ((ScaleTransform)CheckSymbol.RenderTransform).ScaleY = 0;
            TitleTextBlock.Opacity = 0;
            ShowStoryboard.Begin();
        }
    }
}
