using AutoMapper;
using FamilyExperience.Dataimport.Mapping;
using FamilyExperience.Dataimport.Helper;
using Newtonsoft.Json;
using FamilyExperienced.Dataimport.Models.Json;
using FamilyExperience.Dataimport.Models.Json;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OrganisationType;
using FamilyExperience.Dataimport.Models;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyExperience.Dataimport.Models.API;
using FamilyExperience.Dataimport.Service;
using System.Text;
using System.Globalization;

namespace FamilyExperience.Dataimport
{
    public class TransformTHData
    {
        protected static HttpClient _client;
        protected static HttpClient? _apiClient;
        protected PostcodeLocationService _postcodeLocationService;
        private List<LongitudeLatitude> longitudeLatitudes;
        private List<THAvailability> tHAvailabilities;


        public TransformTHData()
        {
            _postcodeLocationService = new PostcodeLocationService();
            longitudeLatitudes = new List<LongitudeLatitude>();
            tHAvailabilities = new List<THAvailability>();

        }

        public async Task ProcessDataAsync()
        {
            try
            {                
                var mapper = GetMapper();
                THRoot ThRoot = new THRoot();
                              
                var mainTHServices = ExcelReader.ReadExcel(@"D:\DFE\TH\AP_MainSet.xlsx");
                ThRoot.services = JsonConvert.DeserializeObject<List<THService>>(mainTHServices);

                var availTHData = ExcelReader.ReadExcel(@"D:\DFE\TH\All_ProvidersV4V2_Availabilty.xlsx");
                tHAvailabilities = JsonConvert.DeserializeObject<List<THAvailability>>(availTHData);

                var orgid = "";

                var test = ThRoot.services.Take(1);

                var openReferralOrgRecord = new OpenReferralOrganisationWithServicesDto() { Name = "Tower Hamlets council", Url = "https://www.towerhamlets.gov.uk/", Description = "Tower Hamlets council" };
                openReferralOrgRecord.Id = Guid.NewGuid().ToString();
                openReferralOrgRecord.OrganisationType = GetOrganisationType();

                foreach (var service in test)
                {
                    service.OrganisationId = openReferralOrgRecord.Id;
                    service.ServiceType = GetServiceType();
                    service.Id = Guid.NewGuid().ToString();

                    // contacts
                    service.ContactDetails = GetContactDetails(service);

                    // cost option
                   service.CostOptions = GetCostOptions(service);

                    //location
                    service.ServiceAtLocations = GetServiceAtLocations(service);                   

                }

                var openReferralRecord = mapper.Map<List<OpenReferralServiceDto>>(test);
                openReferralOrgRecord.Services = openReferralRecord;

                //new Uri("https://localhost:7022/")  

                _apiClient = new HttpClient
                {
                    BaseAddress = new Uri("https://localhost:7022/")

                };
                var apiRequest = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_apiClient.BaseAddress + $"api/organizations"),
                    Content = new StringContent(JsonConvert.SerializeObject(openReferralOrgRecord),
                    Encoding.UTF8, "application/json")
                };
                using var isapiResponse = await _apiClient.SendAsync(apiRequest);

                Console.WriteLine(isapiResponse.StatusCode);

            }
            catch (Exception ex) { }
        }


        private List<RegularSchedule> GetSchedules(THService service)
        {
            var serviceSchedules = tHAvailabilities.Where(t=>t.ProviderId== service.ProviderId).ToList();

            var schedules = new List<RegularSchedule>();
            foreach (var serviceSchedule in serviceSchedules)
            {
                schedules.Add(new RegularSchedule()
                { Id = Guid.NewGuid().ToString(),
                    ServiceId = service.Id,
                    Byday = serviceSchedule.AvailableDay,
                    OpensAt = DateTime.ParseExact(serviceSchedule.StartTime, "h:mm tt", CultureInfo.InvariantCulture).ToUniversalTime(),
                    ClosesAt = DateTime.ParseExact(serviceSchedule.EndTime, "h:mm tt", CultureInfo.InvariantCulture).ToUniversalTime(),

                }); ;
            }

            return schedules;
        }



        private List<ServiceAtLocation> GetServiceAtLocations(THService service)
        {

            var serviceAtLocations = new List<ServiceAtLocation>();
            serviceAtLocations.Add(new ServiceAtLocation()
            {
                Id = Guid.NewGuid().ToString(),
                LocationDetails = GetLocations(service), 
               // RegularSchedules = GetSchedules(service)
                
            });

            return serviceAtLocations;
        }

        private Location GetLocations(THService service)
        {
            Location location = new Location();
            location.Id = Guid.NewGuid().ToString();
            location.Name = service.LocationName;
            location.Description = service.LocationName;

            var longlatDetails = GetLongitudeLatitudeForPostcode(service.Postcode);

            location.Latitude = longlatDetails.Latitude;
            location.Longitude = longlatDetails.Longitude;

            //Physical Address
            location.PhysicalAddresses = GetPhysicalAddress(service);
            return location;
        }

        public LongitudeLatitude? GetLongitudeLatitudeForPostcode(string postcode)
        {
            var longitudeLatitude = new LongitudeLatitude();
            if (longitudeLatitudes is not null && longitudeLatitudes.Count > 0)
            {
                var details = longitudeLatitudes.Where(x => x.Postcode == postcode).ToList();
                return details.Count >= 1 ? longitudeLatitude = details.FirstOrDefault() :
                   PostCodeLookUp(postcode, longitudeLatitude);

            }
            else { return PostCodeLookUp(postcode, longitudeLatitude); }


        }

        private LongitudeLatitude PostCodeLookUp(string postcode, LongitudeLatitude longitudeLatitude)
        {
            var result = _postcodeLocationService.LookupPostcode(postcode).Result;
            if (result.result is null) { return new LongitudeLatitude(); }
            longitudeLatitude.Postcode = result.result.postcode;
            longitudeLatitude.Latitude = result.result.latitude;
            longitudeLatitude.Longitude = result.result.longitude;
            longitudeLatitude.AdministractiveDistrictCode = result.result.codes.admin_district;
            longitudeLatitudes.Add(longitudeLatitude);
            return longitudeLatitude;


        }


        private static Mapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMappingTHProfiles());

            });
            return new Mapper(config);

        }

        private List<CostOption> GetCostOptions(THService service)
        {
            var CostOptions = new List<CostOption>();

            if (service.ProviderFree == "Free")
            {
                CostOptions.Add(GetCost("0", "Free"));
            }
            if (!string.IsNullOrEmpty(service.ProviderCostPerHour)){ CostOptions.Add(GetCost(service.ProviderCostPerHour,"per hour")); }
            if (!string.IsNullOrEmpty(service.ProviderCostPerSession)) { CostOptions.Add(GetCost(service.ProviderCostPerHour, "per session")); }
            if (!string.IsNullOrEmpty(service.ProviderCostPerDay)) { CostOptions.Add(GetCost(service.ProviderCostPerHour, "per day")); }
            if (!string.IsNullOrEmpty(service.ProviderCostPerWeek)) { CostOptions.Add(GetCost(service.ProviderCostPerHour, "per week")); }
            if (!string.IsNullOrEmpty(service.ProviderCostPerHour)) { CostOptions.Add(GetCost(service.ProviderCostPerHour, "per term")); }

            return CostOptions;
           
        }

        private CostOption GetCost(string providercost, string option)
        {
            decimal costDecimal;
            return new CostOption()
            {
                Id = Guid.NewGuid().ToString(),
                Amount = decimal.TryParse(providercost, out costDecimal) ? costDecimal : 0,
                Amount_description = option
            };
        }
         


    private List<ContactDetails> GetContactDetails(THService service)
        {
            var ContactDetails = new List<ContactDetails>();
            ContactDetails.Add(new ContactDetails()
            {
                Id = Guid.NewGuid().ToString(),
                Name = service.LocationName, // check on this
                Numbers = GetContactNumbers(service)
            }); ;

            return ContactDetails;
        }

        private List<PhysicalAddress> GetPhysicalAddress(THService service)
        {
            List<PhysicalAddress> PhysicalAddresses = new List<PhysicalAddress>();

            //CheckIfLocationExists(service.Postcode);

            PhysicalAddresses.Add(new PhysicalAddress()
            {
                Address_1 = string.IsNullOrEmpty(service.Address) ? $"{service.Street}, {service.Town}" : $"{service.Address}, {service.Street},{service.Town}",
                City = service.Town,
                Postal_code = service.Postcode,
                Country = "UK",
                Id = Guid.NewGuid().ToString()

            });
            return PhysicalAddresses;

        }

        private static OrganisationTypeDto GetOrganisationType()
        {
            return new OrganisationTypeDto("1", "LA", "Local Authority");
        }

        public List<ContactNumbers> GetContactNumbers(THService service)
        {
            List<ContactNumbers> contactNumbers = new();
            
                contactNumbers.Add(new ContactNumbers() { Number = service.Number, Id = Guid.NewGuid().ToString() });
              if(!string.IsNullOrEmpty(service.Mobile))   contactNumbers.Add(new ContactNumbers() { Number = service.Mobile, Id = Guid.NewGuid().ToString() });
            return contactNumbers;
        }

        private ServiceType GetServiceType()
        {
            return new ServiceType() { Id = "2", Name = "FX", Description = "Family Experience" };
        }
    }
}
