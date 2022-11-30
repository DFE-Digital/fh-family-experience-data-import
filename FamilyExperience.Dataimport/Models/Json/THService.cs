using FamilyExperience.Dataimport.Models;
using FamilyExperience.Dataimport.Models.API;
using Newtonsoft.Json;

namespace FamilyExperienced.Dataimport.Models.Json
{
    public class THRoot
    {
        public List<THService> services { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string Id { get; set; }

    }


    public class THService
{
        [JsonProperty("provider_id")]
        public string ProviderId { get; set; }

        [JsonProperty("reg_name")]
        public string LocationName { get; set; }

        [JsonProperty("provider_name")]
        public string ServiceName { get; set; }

        [JsonProperty("provider_service_description")]
        public string ServiceDescription { get; set; }

        [JsonProperty("provider_start")]
        public string ValidFrom { get; set; }

        [JsonProperty("provider_end")]
        public string ValidTo { get; set; }

        [JsonProperty("provider_telephone")]
        public string Number { get; set; }

        [JsonProperty("provider_mobile")]
        public string Mobile { get; set; }



        [JsonProperty("provider_web_site")]
        public string Url { get; set; }

        [JsonProperty("provider_email")]
        public string Email { get; set; }

        [JsonProperty("ADR_NUMBER")]
        public string Address { get; set; }

        [JsonProperty("ADR_STREET")]
        public string Street { get; set; }

        [JsonProperty("ADR_TOWN")]
        public string Town { get; set; }

        [JsonProperty("ADR_COUNTY")]
        public string County { get; set; }

        [JsonProperty("ADR_POSTCODE")]
        public string Postcode { get; set; }

        [JsonProperty("provider_vac_agefrom")]
        public string MinimumAge { get; set; }

        [JsonProperty("provider_vac_ageto")]
        public string MaximumAge { get; set; }

        [JsonProperty("ProviderFree")]
        public string ProviderFree { get; set; }

        [JsonProperty("provider_costsess")]
        public string ProviderCostPerSession { get; set; }

        [JsonProperty("provider_costhour")]
        public string ProviderCostPerHour { get; set; }

        [JsonProperty("provider_costday")]
        public string ProviderCostPerDay { get; set; }

        [JsonProperty("provider_costweek")]
        public string ProviderCostPerWeek { get; set; }

        [JsonProperty("provider_costterm")]
        public string ProviderCostPerTerm { get; set; }

        public string Id { get; set; }

        public string OrganisationId { get; set; }
        public List<ContactDetails> ContactDetails { get; set; }

        public List<CostOption> CostOptions { get; set; }

        public List<ServiceAtLocation> ServiceAtLocations { get; set; }       

        public ServiceType ServiceType { get; set; }

        public bool CanFamilyChooseDeliveryLocation { get; set; } = false;


    }
}
