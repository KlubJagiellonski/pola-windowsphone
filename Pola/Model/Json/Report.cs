using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pola.Model.Json
{
    public class Report
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }
    }
}
