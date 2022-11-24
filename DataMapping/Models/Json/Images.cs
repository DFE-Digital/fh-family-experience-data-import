
using Newtonsoft.Json;

namespace DataMapping.Models
{
    public class Images
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
