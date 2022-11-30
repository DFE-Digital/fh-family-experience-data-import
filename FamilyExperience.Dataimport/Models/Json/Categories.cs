using Newtonsoft.Json;


namespace FamilyExperience.Dataimport.Models.Json
{
    public class Categories
    {
        [JsonProperty("provtype_desc")] 

        public string THCategory { get; set; }

        [JsonProperty("National Category - L1")]
        public string Category { get; set; }

        [JsonProperty("National Category - L2")]
        public string SubCategory { get; set; }

    }
}
