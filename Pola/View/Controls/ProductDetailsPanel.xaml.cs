﻿using System;
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
        private bool isOpen;
        private double openPosition = 400;

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

        public ProductDetailsPanel()
        {
            this.InitializeComponent();
        }

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
        }

        public void Close()
        {
            FadeOutTranslateAnimation.To = openPosition;
            FadeOutSotryboard.Begin();
            isOpen = false;
            ContentGrid.IsHitTestVisible = isOpen;
        }

        private void DismissLayer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Close();
        }
    }
}