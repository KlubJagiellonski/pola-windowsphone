using Newtonsoft.Json;

namespace Pola.Model.Json
{
    public class Company
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("plCapital")]
        public int? PlCapital { get; set; }

        [JsonProperty("plCapital_notes")]
        public string PlCapitalNotes { get; set; }

        [JsonProperty("plWorkers")]
        public int? PlWorkers { get; set; }

        [JsonProperty("plWorkers_notes")]
        public string PlWorkersNotes { get; set; }

        [JsonProperty("plRnD")]
        public int? PlRnD { get; set; }

        [JsonProperty("plRnD_notes")]
        public string PlRnDNotes { get; set; }

        [JsonProperty("plRegistered")]
        public int? PlRegistered { get; set; }

        [JsonProperty("plRegistered_notes")]
        public string PlRegisteredNotes { get; set; }

        [JsonProperty("plNotGlobEnt")]
        public int? PlNotGlobalEntity { get; set; }

        [JsonProperty("plNotGlobEnt_notes")]
        public string PlNotGlobalEntityNotes { get; set; }
    }
}
