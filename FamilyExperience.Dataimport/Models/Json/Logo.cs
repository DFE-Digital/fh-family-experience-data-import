using Newtonsoft.Json;

namespace DataMapping.Models
{
    public class Logo
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
