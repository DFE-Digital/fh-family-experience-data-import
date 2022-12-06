using FamilyExperience.Dataimport.Helper;
using Newtonsoft.Json;
using FamilyExperienced.Dataimport.Models.Json;
using FamilyExperience.Dataimport.Models.Json;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OrganisationType;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyExperience.Dataimport.Models.API;
using FamilyExperience.Dataimport.Service;
using System.Text;
using System.Globalization;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralRegularSchedule;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralContacts;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhones;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.ServiceType;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralCostOptions;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceAtLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhysicalAddresses;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralEligibilitys;

namespace FamilyExperience.Dataimport
{
    public class TransformTHData
    {
        protected static HttpClient _client;
        private static HttpClient? _apiClient;
        private PostcodeLocationService _postcodeLocationService;
        private List<LongitudeLatitude> longitudeLatitudes;
        private List<THAvailability> tHAvailabilities;
        private List<Categories> categoriesList;
        private List<Taxonomy> masterTaxonomies;


        public TransformTHData()
        {
            _postcodeLocationService = new PostcodeLocationService();
            longitudeLatitudes = new List<LongitudeLatitude>();
            tHAvailabilities = new List<THAvailability>();
            categoriesList = new List<Categories>();
            masterTaxonomies = new List<Taxonomy>();

        }

        public async Task ProcessDataAsync()
        {
            try
            {
                var orgid = "4faa3eb4-709b-4731-a5ed-dafaa6bf93c5";
               // var mapper = GetMapper();
                var ThRoot = new THRoot();

                var mainTHServices = ExcelReader.ReadExcel(@"D:\DFE\TH\AP_MainSet.xlsx");
                ThRoot.services = JsonConvert.DeserializeObject<List<THService>>(mainTHServices);

                var availTHData = ExcelReader.ReadExcel(@"D:\DFE\TH\All_ProvidersV4V2_Availabilty.xlsx");
                tHAvailabilities = JsonConvert.DeserializeObject<List<THAvailability>>(availTHData);

                var THCategories = ExcelReader.ReadExcel(@"D:\DFE\TH\All_ProvidersV4V2_Availabilty.xlsx");
                categoriesList = JsonConvert.DeserializeObject<List<Categories>>(THCategories);

                masterTaxonomies = await GetMasterTaxonomy();

                var test = ThRoot.services.Take(3);

                var openReferralOrgRecord = new OpenReferralOrganisationWithServicesDto() { Name = "Tower Hamlets council", Url = "https://www.towerhamlets.gov.uk/", Description = "Tower Hamlets council" };
                openReferralOrgRecord.Id = orgid;//Guid.NewGuid().ToString();
                openReferralOrgRecord.OrganisationType = GetOrganisationType();
                openReferralOrgRecord.Services = new List<OpenReferralServiceDto>();

                foreach (var service in test)
                {
                    openReferralOrgRecord.Services.Add(
                         new OpenReferralServiceDto()
                         {
                             Id = Guid.NewGuid().ToString(),
                             Name = service.ServiceName,
                             Url = service.Url,
                             Description = service.ServiceDescription,
                             Email = service.Email,
                             CanFamilyChooseDeliveryLocation = false,
                             OpenReferralOrganisationId = openReferralOrgRecord.Id,
                             ServiceType = GetServiceType(),
                             Contacts = GetContactDetails(service),
                             Cost_options = GetCostOptions(service),
                             Service_at_locations = GetServiceAtLocations(service)
                             
                             
                         });
                    openReferralOrgRecord.AdministractiveDistrictCode = GetLongitudeLatitudeForPostcode(service.Postcode).AdministractiveDistrictCode;

                    //service.OrganisationId = openReferralOrgRecord.Id;
                    //service.ServiceType = GetServiceType();
                    //service.Id = Guid.NewGuid().ToString();

                    // contacts
                    // service.ContactDetails = GetContactDetails(service);

                    // cost option
                    // service.CostOptions = GetCostOptions(service);

                    //location
                    // service.ServiceAtLocations = GetServiceAtLocations(service);

                    //  service.ServiceTaxonomies = GetCategories(service);



                }

                //var openReferralRecord = mapper.Map<List<OpenReferralServiceDto>>(test);
                //openReferralOrgRecord.Services = openReferralRecord;

                //new Uri("https://localhost:7022/")  

                _apiClient = new HttpClient
                {
                    BaseAddress = new Uri("https://localhost:7022/")

                };
                var apiRequest = new HttpRequestMessage()
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(_apiClient.BaseAddress + $"api/organizations/{orgid}"),
                    Content = new StringContent(JsonConvert.SerializeObject(openReferralOrgRecord),
                    Encoding.UTF8, "application/json")
                };
                using var isapiResponse = await _apiClient.SendAsync(apiRequest);

                Console.WriteLine(isapiResponse.StatusCode);

            }
            catch (Exception ex) { }
        }

        private static async Task<List<Taxonomy>> GetMasterTaxonomy()
        {
           var dfeTaxonomyMasterList = await _apiClient.GetAsync(new Uri(_apiClient.BaseAddress + $"api/taxonomies"));
            var apiResponse = await dfeTaxonomyMasterList.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Taxonomy>>(apiResponse);
        }

        private List<ServiceTaxonomy> GetCategories(THService service)
        {

            var DfeCategory = categoriesList.Where(k => k.Category == service.Category).First();
            var cat = masterTaxonomies.Where(s=>s.Name == DfeCategory.SubCategory).First();

            var categories = new List<ServiceTaxonomy>();
            categories.Add(new ServiceTaxonomy()
            {
                Id = Guid.NewGuid().ToString(),
                Taxonomy = cat
            });

            return categories;
        }
        private List<OpenReferralRegularScheduleDto> GetSchedules(THService service)
        {
            var serviceSchedules = tHAvailabilities.Where(t => t.ProviderId == service.ProviderId).ToList();
            var schedules = new List<OpenReferralRegularScheduleDto>();
            foreach (var serviceSchedule in serviceSchedules)
            {
                schedules.Add(new OpenReferralRegularScheduleDto(Guid.NewGuid().ToString(),
                    description: serviceSchedule.AvailableDay,
                    opens_at: DateTime.ParseExact(serviceSchedule.StartTime, "h:mm tt", CultureInfo.InvariantCulture).ToUniversalTime(),
                    closes_at: DateTime.ParseExact(serviceSchedule.EndTime, "h:mm tt", CultureInfo.InvariantCulture).ToUniversalTime(),
                    byday: serviceSchedule.AvailableDay,
                    bymonthday: null,
                    dtstart: null,
                    freq: null,
                    interval: null,
                    valid_from: null,
                    valid_to: null
                    ));
            }
            return schedules;
        }

        private List<OpenReferralServiceAtLocationDto> GetServiceAtLocations(THService service)
        {

            var serviceAtLocations = new List<OpenReferralServiceAtLocationDto>();
            serviceAtLocations.Add(new OpenReferralServiceAtLocationDto()
            {
                Id = Guid.NewGuid().ToString(),
                Location = GetLocations(service),
               Regular_schedule = GetSchedules(service)
            });
            return serviceAtLocations;
        }

        
        private OpenReferralLocationDto GetLocations(THService service)
        {
            var longlatDetails = GetLongitudeLatitudeForPostcode(service.Postcode);
            return new OpenReferralLocationDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = service.LocationName,
                Description = service.LocationName,
                Latitude = longlatDetails.Latitude,
                Longitude = longlatDetails.Longitude,
                Physical_addresses = GetPhysicalAddress(service)

            };           
            
        }


        private List<OpenReferralEligibilityDto> GetEligibilities(THService service)
        {
            var eligibilities = new List<OpenReferralEligibilityDto>();
            eligibilities.Add(new OpenReferralEligibilityDto()
            {
                Id = Guid.NewGuid().ToString(),
                // Maximum_age = service.MaximumAge,
              //   Minimum_age =  service.MinimumAge

            });

            return eligibilities;

        }


        private void GetAge(string age)
        {
            // var t = age.Split()
        }


        private LongitudeLatitude? GetLongitudeLatitudeForPostcode(string postcode)
        {
            var longitudeLatitude = new LongitudeLatitude();
            if (longitudeLatitudes is not null && longitudeLatitudes.Count > 0)
            {
                var details = longitudeLatitudes.Where(x => x.Postcode == postcode).ToList();
                return details.Count >= 1 ? details.FirstOrDefault() : PostCodeLookUp(postcode, longitudeLatitude);

            }
            else
            {
                return PostCodeLookUp(postcode, longitudeLatitude);
            }
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


        private List<OpenReferralCostOptionDto> GetCostOptions(THService service)
        {
            var CostOptions = new List<OpenReferralCostOptionDto>();

            if (service.ProviderFree == "Free")
            {
                CostOptions.Add(GetCost("0", "Free"));
            }
            if (!string.IsNullOrEmpty(service.ProviderCostPerHour)) { CostOptions.Add(GetCost(service.ProviderCostPerHour, "per hour")); }
            if (!string.IsNullOrEmpty(service.ProviderCostPerSession)) { CostOptions.Add(GetCost(service.ProviderCostPerHour, "per session")); }
            if (!string.IsNullOrEmpty(service.ProviderCostPerDay)) { CostOptions.Add(GetCost(service.ProviderCostPerHour, "per day")); }
            if (!string.IsNullOrEmpty(service.ProviderCostPerWeek)) { CostOptions.Add(GetCost(service.ProviderCostPerHour, "per week")); }
            if (!string.IsNullOrEmpty(service.ProviderCostPerTerm)) { CostOptions.Add(GetCost(service.ProviderCostPerHour, "per term")); }

            return CostOptions;

        }

        private OpenReferralCostOptionDto GetCost(string providercost, string option)
        {
            decimal costDecimal;
            return new OpenReferralCostOptionDto()
            {
                Id = Guid.NewGuid().ToString(),
                Amount = decimal.TryParse(providercost, out costDecimal) ? costDecimal : 0,
                Amount_description = option
            };
        }
         
        private List<OpenReferralContactDto> GetContactDetails(THService service)
        {
            var ContactDetails = new List<OpenReferralContactDto>();
            ContactDetails.Add(new OpenReferralContactDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = service.LocationName,
                Phones = GetContactNumbers(service)
            }); ;

            return ContactDetails;
        }     

        private List<OpenReferralPhysicalAddressDto> GetPhysicalAddress(THService service)
        {
            var PhysicalAddresses = new List<OpenReferralPhysicalAddressDto>();

            //CheckIfLocationExists(service.Postcode);


            PhysicalAddresses.Add(new OpenReferralPhysicalAddressDto()
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

        private List<OpenReferralPhoneDto> GetContactNumbers(THService service)
        {
            List<OpenReferralPhoneDto> contactNumbers = new();

            contactNumbers.Add(new OpenReferralPhoneDto() { Number = service.Number, Id = Guid.NewGuid().ToString() });
            if (!string.IsNullOrEmpty(service.Mobile)) contactNumbers.Add(new OpenReferralPhoneDto() { Number = service.Mobile, Id = Guid.NewGuid().ToString() });
            return contactNumbers;
        }

        private ServiceTypeDto GetServiceType()
        {
            return new ServiceTypeDto(id: "2", name: "FX", description: "Family Experience") { };
        }
    }
}
