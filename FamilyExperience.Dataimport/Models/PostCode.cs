using Newtonsoft.Json;

namespace FamilyExperience.DataImport.Models
{
    public class PostCode
    {
        public Result Result { get; set; }
    }

    public class Result
    {
        public string Postcode { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public Codes Codes { get; set; }
    }

    public class Codes
    {
        [JsonProperty("admin_district")]
        public string AdminDistrict { get; set; }
    }
}
