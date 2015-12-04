using Newtonsoft.Json;

namespace Pola.Model.Json
{
    public class ReportResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("signed_requests")]
        public string[][] SignedRequests { get; set; }
    }
}
