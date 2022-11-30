using FamilyExperience.Dataimport.Converter;
using Newtonsoft.Json;
using System.Text.Json.Serialization;


namespace FamilyExperience.Dataimport.Models
{
    public class CostTable
    {
        [JsonProperty("cost_amount")]
        public string CostAmount { get; set; }

        [JsonProperty("cost_type")]
        public CostType CostType { get; set; }
        
    }
   
}
