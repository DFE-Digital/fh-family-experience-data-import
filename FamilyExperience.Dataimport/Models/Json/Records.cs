using FamilyExperience.Dataimport.Converter;
using Newtonsoft.Json;
using FamilyExperience.Dataimport.Models.API;

namespace FamilyExperience.Dataimport.Models
{

    public class Rootobject
    {
        public Record[] records { get; set; }          
        public string Name { get; set; }
        public string Description { get; set; }

        public string Id { get; set; }
        
    }

    public class Record
    { 
        public string Id { get; set; }

        public string OrganisationId { get; set; }
    
        [JsonProperty("venue_name")]
        public string Name { get; set; }

        [JsonProperty("public_address_5")]
        public string PublicAddress5 { get; set; }

        [JsonProperty("contact_name")]        
        public string ContactName { get; set; }

        [JsonProperty("website")]
        [JsonConverter(typeof(ListorSingleConverter<Website>))]
        public List<Website> Website { get; set; }

        [JsonProperty("recordUri")]
        public string RecordUri { get; set; }

        [JsonProperty("public_address_2")]
        public string PublicAddress2 { get; set; }

        [JsonProperty("cost_description")]
        public string CostDescription { get; set; }

        [JsonProperty("contact_telephone")]
        [JsonConverter(typeof(ListorSingleConverter<string>))]
        public List<string> ContactTelephone { get; set; }

        [JsonProperty("public_address_1")]
        public string PublicAddress1 { get; set; }

        [JsonProperty("public_address_4")]
        public string PublicAddress4 { get; set; }

        [JsonProperty("public_address_3")]
        public string PublicAddress3 { get; set; }
        public string externalId { get; set; }

        [JsonProperty("Description")]
        public string description { get; set; }

        [JsonProperty("date_timeofday")]
        [JsonConverter(typeof(ListorSingleConverter<DateTimeofday>))]
        public List<DateTimeofday> DateTimeofDay { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("date_displaydate")]
        public string DateDisplaydate { get; set; }

        [JsonProperty("contact_email")]
        public string Email { get; set; }

        [JsonProperty("venue_postcode")]
        public string Postcode { get; set; }

        [JsonProperty("lastUpdate")]
        public string LastUpdate { get; set; }

        [JsonProperty("notes_public")]
        public string Notes { get; set; }
        public object ecd_timetable_openinghours_list { get; set; }

        [JsonProperty("date_activity_period")]
        public DateActivityPeriod DateActivityPeriod { get; set; }
        public string contact_notes { get; set; }

        [JsonProperty("date_session_info")]
        [JsonConverter(typeof(ListorSingleConverter<string>))]
        public List<string> DatesessionInfo { get; set; }      
        public Images images { get; set; }
        public object files { get; set; }

        [JsonProperty("cost_table")]
        [JsonConverter(typeof(ListorSingleConverter<CostTable>))]        
        public List<CostTable> CostOption { get; set; }

        public List<ContactDetails> ContactDetails { get; set; }

        public List<CostOption> CostOptions { get; set; }

        public List<ServiceAtLocation> ServiceAtLocations { get; set; }   
        
        public ServiceType ServiceType { get; set; }

        public bool CanFamilyChooseDeliveryLocation { get; set; } = false;



    }

   
    public class ContactDetails
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public List<ContactNumbers> Numbers { get; set; }
    }

    public class ContactNumbers
    {
        public string Id { get; set; }
        public string Number { get; set; }
    }

}
