using Newtonsoft.Json;

namespace Pola.Model.Json
{
    public class Report : Attach
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("product_id")]
        public long ProductId { get; set; }

        [JsonProperty("files_count")]
        public int FilesCount { get; set; }

        public Report()
        { }

        public Report(string description, int filesCount, long productId)
        {
            this.Description = description;
            this.FilesCount = filesCount;
            this.ProductId = productId;

            this.MimeType = Attach.DefaultMimeType;
            this.FileExtension = Attach.DefaultFileExtension;
        }
    }
}
