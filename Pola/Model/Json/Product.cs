using Newtonsoft.Json;

namespace Pola.Model.Json
{
    public class Product
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("company")]
        public Company Company { get; set; }

        [JsonProperty("verified")]
        public bool IsVerified { get; set; }

        [JsonProperty("plScore")]
        public int? PlScore { get; set; }

        [JsonProperty("code")]
        public long Code { get; set; }

        public bool IsReported { get; set; }
    }
}
