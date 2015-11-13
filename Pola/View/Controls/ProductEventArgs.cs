using Pola.Model.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pola.View.Controls
{
    public class ProductEventArgs : EventArgs
    {
        public Product Product { get; set; }

        public ProductEventArgs(Product product)
        {
            this.Product = product;
        }
    }
}
