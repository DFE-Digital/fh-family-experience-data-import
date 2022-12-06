using Newtonsoft.Json;

namespace FamilyExperience.Dataimport.Models
{
    public class StandardData
    {
        [JsonProperty("Local Authority")]
        public string LAName { get; set; }

        [JsonProperty("Organisation type")]
        public string OrgType { get; set; }

        [JsonProperty("Name of organisation")]
        public string OrganisationName { get; set; }

        [JsonProperty("Name of service")]
        public string ServiceName { get; set; }

        [JsonProperty("Admin email for managing the service")]
        public string Email { get; set; }

        [JsonProperty("Delivery method")]
        public string DeliveryMethod { get; set; }

        [JsonProperty("Location name")]
        public string LocationName { get; set; }

        [JsonProperty("Location description")]
        public string LocationDescription { get; set; }

        [JsonProperty("Location type")]
        public string LocationType { get; set; }

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

        [JsonProperty("Email to contact service")]
        public string EmailToContactService { get; set; }

        [JsonProperty("Phone number for people wanting to phone the service")]
        public string PhoneToContactService { get; set; }

        [JsonProperty("Website")]
        public string Website { get; set; }

        [JsonProperty("Phone number for people wanting to text the service")]
        public string TextToContactService { get; set; }

        [JsonProperty("Sub- Category")]
        public string Category { get; set; }

        [JsonProperty("Cost")]
        public string Cost { get; set; }

        [JsonProperty("Cost (£ in pounds)")]
        public string CostInPounds { get; set; }

        [JsonProperty("Cost per")]
        public string CostPerUnit { get; set; }

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
