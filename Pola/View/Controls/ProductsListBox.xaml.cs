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
        #region Constatns

        private const int MaxCount = 4;

        #endregion

        #region Events

        public event EventHandler<ProductEventArgs> ProductSelected;
        private void OnProductSelected(ProductItem productItem)
        {
            if (ProductSelected != null)
                ProductSelected(productItem, new ProductEventArgs(productItem.Product));
        }

        #endregion

        #region Constructor

        public ProductsListBox()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Adds a new collapsed product item to the top of products list.
        /// </summary>
        /// <param name="barcode"></param>
        public void AddProduct(string barcode)
        {
            if (RootGrid.Children.Count > MaxCount)
                return;

            if (ContainsProduct(barcode))
            {
                MoveProductItemToTop(GetProductItem(barcode));
                return;
            }

            ProductItem productItem = new ProductItem(barcode);
            productItem.VerticalAlignment = VerticalAlignment.Bottom;
            productItem.Tapped += (sender, e) =>
                {
                    OnProductSelected(productItem);
                    //MoveProductItemToTop(productItem);
                };

            double y = -RootGrid.Children.Count * (ProductItem.DefaultHeight + ProductItem.Space);
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
                                return ((ProductItem)item).Translation.Y;
                            });
                    };
                itemToRemove.Hide();
                foreach (ProductItem item in RootGrid.Children)
                    item.SlideDown();
            }
        }

        private void MoveProductItemToTop(ProductItem productItem)
        {
            if (!RootGrid.Children.Contains(productItem))
                return;

            int aboveItemsCount = 0;
            foreach (ProductItem item in RootGrid.Children)
                if (item.Translation.Y < productItem.Translation.Y)
                {
                    aboveItemsCount++;
                    item.SlideDown();
                }
            productItem.SlideUp(aboveItemsCount);
            RootGrid.Children.Move((uint)RootGrid.Children.IndexOf(productItem), (uint)RootGrid.Children.Count - 1);
        }

        private bool ContainsProduct(string barcode)
        {
            foreach (ProductItem item in RootGrid.Children)
                if (item.Barcode.Equals(barcode))
                    return true;

            return false;
        }

        public ProductItem GetProductItem(string barcode)
        {
            foreach (ProductItem item in RootGrid.Children)
                if (item.Barcode.Equals(barcode))
                    return item;
            return null;
        }

        public ProductItem GetProductItem(Product product)
        {
            foreach (ProductItem productItem in RootGrid.Children)
                if (productItem.Product == product)
                    return productItem;
            return null;
        }

        #endregion
    }
}
