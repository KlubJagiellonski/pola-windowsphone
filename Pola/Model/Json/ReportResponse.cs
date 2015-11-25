using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
