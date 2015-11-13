using Pola.Model.Json;
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
    public sealed partial class ProductsListBox : UserControl
    {
        private const int MaxCount = 4;

        public ProductsListBox()
        {
            this.InitializeComponent();
        }

        /// <summary>
        ///  Adds a new collapsed product item to the top of products list.
        /// </summary>
        /// <param name="barcode"></param>
        public void AddProduct(string barcode)
        {
            if (RootGrid.Children.Count > MaxCount)
                return;

            ProductItem productItem = new ProductItem();
            productItem.VerticalAlignment = VerticalAlignment.Top;
            productItem.Tapped += (sender, e) =>
                {
                    int aboveItemsCount = 0;
                    foreach (ProductItem item in RootGrid.Children)
                        if (item.Position.Y < productItem.Position.Y)
                        {
                            aboveItemsCount++;
                            item.SlideDown();
                        }
                    productItem.SlideUp(aboveItemsCount);
                    RootGrid.Children.Move((uint)RootGrid.Children.IndexOf(productItem), (uint)RootGrid.Children.Count - 1);
                };

            double y = this.ActualHeight - (RootGrid.Children.Count + 1) * (ProductItem.CollapsedHeight + ProductItem.Space);
            ((CompositeTransform)productItem.RenderTransform).TranslateY = y;
            RootGrid.Children.Add(productItem);

            if (RootGrid.Children.Count > MaxCount)
            {
                ProductItem itemToRemove = (ProductItem)RootGrid.Children.First();
                itemToRemove.Hidden += (sender, e) =>
                    {
                        RootGrid.Children.Remove(itemToRemove);
                        RootGrid.Children.OrderBy((item) =>
                            {
                                return ((ProductItem)item).Position.Y;
                            });
                    };
                itemToRemove.Hide();
                foreach (ProductItem item in RootGrid.Children)
                    item.SlideDown();
            }
        }

        private bool ContainsProduct(string barcode)
        {
            foreach (ProductItem item in RootGrid.Children)
                if (item.Barcode.Equals(barcode))
                    return true;

            return false;
        }
    }
}
