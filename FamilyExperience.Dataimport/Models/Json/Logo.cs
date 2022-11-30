using Newtonsoft.Json;

namespace FamilyExperience.Dataimport.Models
{
    public class Logo
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
