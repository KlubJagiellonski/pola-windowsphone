using Newtonsoft.Json;

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
