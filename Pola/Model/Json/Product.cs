using Newtonsoft.Json;
using System.Collections.Generic;

namespace Pola.Model.Json
{
    public class Product
    {
        [JsonProperty("product_id")]
        public long? Id { get; set; }

        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("card_type")]
        public string CardTypeName { get; set; }

        [JsonProperty("plScore")]
        public int? PlScore { get; set; }

        [JsonProperty("altText")]
        public string AltText { get; set; }

        [JsonProperty("plCapital")]
        public int? PlCapital { get; set; }

        [JsonProperty("plWorkers")]
        public int? PlWorkers { get; set; }

        [JsonProperty("plRnD")]
        public int? PlRnD { get; set; }

        [JsonProperty("plRegistered")]
        public int? PlRegistered { get; set; }

        [JsonProperty("plNotGlobEnt")]
        public int? PlNotGlobalEntity { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("sources")]
        public Dictionary<string, string> Links { get; set; }

        [JsonProperty("report_text")]
        public string ReportText { get; set; }

        [JsonProperty("report_button_text")]
        public string ReportButtonText { get; set; }

        [JsonProperty("report_button_type")]
        public string ReportButtonTypeName { get; set; }

        public bool IsReported { get; set; }

        public CardType CardType
        {
            get
            {
                switch (CardTypeName)
                {
                    case "type_white":
                        return CardType.White;
                    case "type_grey":
                    default:
                        return CardType.Grey;
                }
            }
        }
    }

    public enum CardType
    {
        White,
        Grey,
    }
}
