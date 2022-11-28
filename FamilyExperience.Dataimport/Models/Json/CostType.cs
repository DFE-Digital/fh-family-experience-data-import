
using Newtonsoft.Json;

namespace DataMapping.Models
{
    public class CostType
    {
        [JsonProperty("displayName")]
        public string Displayname { get; set; }
    }
}
