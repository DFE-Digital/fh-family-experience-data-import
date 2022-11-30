using Newtonsoft.Json;

namespace FamilyExperience.Dataimport.Models
{
    public class DateActivityPeriod
    {
        [JsonProperty("alwayson")]
        public string AlwaysOn { get; set; }

        [JsonProperty("weekdays")]
        public string[] Weekdays { get; set; }

        [JsonProperty("dates")]
        public string[] Dates { get; set; }
    }

}
