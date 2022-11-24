using Newtonsoft.Json;

namespace DataMapping.Models
{
    public class DateTimeofday
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }

}
