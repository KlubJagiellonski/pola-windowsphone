using Newtonsoft.Json;

namespace Pola.Model.Json
{
    public class AttachResponse
    {
        [JsonProperty("signed_request")]
        public string[] SignedRequest { get; set; }
    }
}
