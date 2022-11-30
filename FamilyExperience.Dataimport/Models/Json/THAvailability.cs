using Newtonsoft.Json;

namespace FamilyExperience.Dataimport.Models.Json
{
    public class THAvailability
    {
        [JsonProperty("provider_id")]
        public string ProviderId { get; set; }

        [JsonProperty("provider_name")]
        public string ProviderName { get; set; }

        [JsonProperty("reg_id")]
        public string RegistrationId { get; set; }

        [JsonProperty("reg_name")]
        public string RegistrationName { get; set; }

        [JsonProperty("pavail_day")]
        public string AvailableDay { get; set; }

        [JsonProperty("pavail_start")]
        public string StartTime { get; set; }

        [JsonProperty("pavail_end")]
        public string EndTime { get; set; }


    }
}
