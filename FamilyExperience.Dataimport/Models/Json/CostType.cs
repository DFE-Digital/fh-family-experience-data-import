
using Newtonsoft.Json;

namespace FamilyExperience.Dataimport.Models
{
    public class CostType
    {
        [JsonProperty("displayName")]
        public string Displayname { get; set; }
    }
}
