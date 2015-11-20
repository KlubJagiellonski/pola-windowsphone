using Pola.Model.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pola.View.Common
{
    public class ProductEventArgs : EventArgs
    {
        public Product Product { get; private set; }

        public ProductEventArgs(Product product)
        {
            this.Product = product;
        }
    }
}
