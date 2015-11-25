using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pola.Model.Json
{
    public class Attach
    {
        public const string DefaultMimeType = "image/png";
        public const string DefaultFileExtension = "png";

        [JsonProperty("mime_type")]
        public string MimeType { get; set; }

        [JsonProperty("file_ext")]
        public string FileExtension { get; set; }

        public static Attach Default
        {
            get
            {
                return new Attach()
                {
                    MimeType = DefaultMimeType,
                    FileExtension = DefaultFileExtension,
                };
            }
        }
    }
}
