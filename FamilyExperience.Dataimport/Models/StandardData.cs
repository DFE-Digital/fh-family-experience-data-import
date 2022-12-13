using Newtonsoft.Json;

namespace FamilyExperience.DataImport.Models
{
    public class StandardData
    {
        [JsonProperty("Local authority")]
        public string LAName { get; set; }

        [JsonProperty("Organisation type")]
        public string OrgType { get; set; }

        [JsonProperty("Name of organisation")]
        public string OrganisationName { get; set; }

        [JsonProperty("Service unique identifier")]
        public string ServiceId { get; set; }

        [JsonProperty("Name of service")]
        public string ServiceName { get; set; }

        [JsonProperty("Service administrator email")]
        public string Email { get; set; }

        [JsonProperty("Delivery method")]
        public string DeliveryMethod { get; set; }

        [JsonProperty("Location name")]
        public string LocationName { get; set; }

        [JsonProperty("Location description")]
        public string LocationDescription { get; set; }

        [JsonProperty("Address line 1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("Address line 2")]
        public string AddressLine2 { get; set; }

        [JsonProperty("Town or City")]
        public string TownOrCity { get; set; }

        [JsonProperty("County")]
        public string County { get; set; }

        [JsonProperty("Postcode")]
        public string Postcode { get; set; }

        [JsonProperty("Contact email")]
        public string ContactEmail { get; set; }

        [JsonProperty("Contact phone")]
        public string ContactPhone { get; set; }

        [JsonProperty("Website")]
        public string Website { get; set; }

        [JsonProperty("Contact sms")]
        public string TextToContactService { get; set; }

        [JsonProperty("Sub-category")]
        public string Category { get; set; }

        [JsonProperty("Cost")]
        public string Cost { get; set; }

        [JsonProperty("Cost (£ in pounds)")]
        public string CostInPounds { get; set; }

        [JsonProperty("Cost per")]
        public string CostPerUnit { get; set; }

        [JsonProperty("Cost Description")]
        public string CostDescription { get; set; }        

        [JsonProperty("Language")]
        public string Language { get; set; }

        [JsonProperty("Is this service for children and young people?")]
        public string ChildOrYoung { get; set; }

        [JsonProperty("Age from")]
        public string MinAge { get; set; }

        [JsonProperty("Age to")]
        public string MaxAge { get; set; }

        [JsonProperty("Opening hours description")]
        public string OpeningHoursDescription { get; set; }

        [JsonProperty("More Details (service description)")]
        public string ServiceDescription { get; set; }
    }
}
