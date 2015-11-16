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

        [JsonProperty("report")]
        public object ReportObject { get; set; }

        public bool AskForReport
        {
            get
            {
                if (ReportObject == null)
                    return false;
                if (ReportObject is bool)
                    return (bool)ReportObject;
                if (ReportObject is string && ((string)ReportObject).Equals("ask_for_company"))
                    return true;
                return false;
            }
        }

        [JsonProperty("code")]
        public long Code { get; set; }
    }
}
