
using Newtonsoft.Json;

namespace FamilyExperience.Dataimport.Models.Json
{
    public class CostType
    {
        [JsonProperty("displayName")]
        public string Displayname { get; set; }
    }
}
