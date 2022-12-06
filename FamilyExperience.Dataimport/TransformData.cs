using FamilyExperience.Dataimport.Helper;
using FamilyExperience.Dataimport.Models.Json;
using Newtonsoft.Json;
using System.Web;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyExperience.Dataimport.Service;
using System.Text;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralRegularSchedule;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralContacts;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhones;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.ServiceType;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralCostOptions;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceAtLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhysicalAddresses;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralEligibilitys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OrganisationType;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceTaxonomys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using FamilyExperience.Dataimport.Models;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceDeliverysEx;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLanguages;

namespace FamilyExperience.Dataimport
{
    public class TransformData
    {
        private readonly LongitudeLatitudeFinder _longitudeLatitudeFinder;
        private readonly PostcodeLocationService _postcodeLocationService;
        private List<OpenReferralTaxonomyDto> masterTaxonomies;
        private List<OpenReferralOrganisationDto> _masterOrgs;
        private OpenReferralOrganisationWithServicesDto _openReferralOrganisationWithServicesDtos;
        private static HttpClient? _apiClient;
        protected readonly string FamilyHub = "Family Hub";
        public TransformData()
        {
            _longitudeLatitudeFinder = new LongitudeLatitudeFinder();
            _postcodeLocationService = new PostcodeLocationService();
            masterTaxonomies = new List<OpenReferralTaxonomyDto>();
            _apiClient = new HttpClient
            {
               // BaseAddress = new Uri("https://localhost:7022/")
                BaseAddress = new Uri("https://s181d01-as-fh-sd-api-dev.azurewebsites.net/")
            };
           
        }


        public async Task ProcessDataAsync()
        {
            try
            {
                string orgid;
                masterTaxonomies = await GetMasterTaxonomy();
                _masterOrgs = await GetOrganisations();


                var path = HttpUtility.HtmlEncode("https://educationgovuk-my.sharepoint.com/:x:/r/personal/ben_macinnes_education_gov_uk/Documents/Documents/Local%20Authority%20Data%20Capture.xlsm?d=waa7a87fc5c354244ba52d29178afebb4&csf=1&web=1&e=9hs4DI");

                var mainTHServices = ExcelReader.ReadExcel(@"D:\DFE\Local Authority Data Capture v3.0.xlsm");
                var services = JsonConvert.DeserializeObject<List<StandardData>>(mainTHServices);
                services = services.Where(k => k.OrganisationName != "").ToList();

                ////var openReferralOrgRecord = new OpenReferralOrganisationWithServicesDto() { Name = "Tower Hamlets council", Url = "https://www.towerhamlets.gov.uk/", Description = "Tower Hamlets council" };
                //openReferralOrgRecord.Id = orgid;//Guid.NewGuid().ToString();
                //openReferralOrgRecord.OrganisationType = GetOrganisationType();
                //openReferralOrgRecord.Services = new List<OpenReferralServiceDto>();

                var orgs = services.Where(k => k.OrganisationName is not null).ToList();
                var Services = new List<OpenReferralServiceDto>();




                //orgid = openReferralOrganisationDto.Id;
                //var openReferralOrgRecord = new OpenReferralOrganisationWithServicesDto()
                //{
                //    Name = service.OrganisationName,
                //    Id = openReferralOrganisationDto.Id, //Guid.NewGuid().ToString(),
                //    OrganisationType = GetOrganisationType(service.LAName),
                //    AdministractiveDistrictCode = _longitudeLatitudeFinder.GetLongitudeLatitudeForPostcode(service.Postcode, _postcodeLocationService).AdministractiveDistrictCode,
                //    Url = service.Website
                //};


                // var orgServicesList = services.Where(t => t.OrganisationName == service.OrganisationName).ToList();
                foreach (var orgService in orgs)
                {
                    var openReferralOrganisationDto = GetOrganisationInfo(orgService.OrganisationName);

                    if (orgService.OrgType == "FamilyHub" && openReferralOrganisationDto == null)
                    {

                        var openReferralOrgRecord = new OpenReferralOrganisationDto()
                        {
                            Name = orgService.OrganisationName,
                            Id = Guid.NewGuid().ToString(),
                            OrganisationType = GetOrganisationType(orgService.OrgType),
                            AdministractiveDistrictCode = _longitudeLatitudeFinder.GetLongitudeLatitudeForPostcode(orgService.Postcode, _postcodeLocationService).AdministractiveDistrictCode,
                            Url = orgService.Website
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
                        _masterOrgs = await GetOrganisations();


                    }




                    openReferralOrganisationDto = GetOrganisationInfo(orgService.OrganisationName);


                    _openReferralOrganisationWithServicesDtos = await GetServicesForOrganisation(openReferralOrganisationDto.Id);
                    var existingService = _openReferralOrganisationWithServicesDtos.Services.FirstOrDefault(x => x.Name == orgService.ServiceName);
                    if (existingService != null && !string.IsNullOrEmpty(existingService.Id))
                    {
                      await   UpdateService(existingService);
                    }
                    else
                    {
                        var service = CreateOpenReferralServiceDto(orgService, openReferralOrganisationDto.Id, null);


                        var apiRequest = new HttpRequestMessage()

                        {
                            Method = HttpMethod.Post,
                            RequestUri = new Uri(_apiClient.BaseAddress + $"api/services"),
                            Content = new StringContent(JsonConvert.SerializeObject(service),
              Encoding.UTF8, "application/json")
                        };
                        using var isapiResponse = await _apiClient.SendAsync(apiRequest);
                        Console.WriteLine(isapiResponse.StatusCode);

                    }

                }
                               

              

            }
            catch (Exception ex) { 
                Console.Write(ex.Message); }
        }

        private async Task UpdateService(OpenReferralServiceDto service)
        {
            try
            {
                var apiRequest = new HttpRequestMessage()
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(_apiClient.BaseAddress + $"api/services/{service.Id}"),
                    Content = new StringContent(JsonConvert.SerializeObject(service),
                        Encoding.UTF8, "application/json")
                };
                var isapiResponse = await _apiClient.SendAsync(apiRequest);
                
                Console.WriteLine(isapiResponse.StatusCode);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message); }
        }

        private OpenReferralServiceDto CreateOpenReferralServiceDto(StandardData orgService, string orgid, string? serviceId)
        {

            return new OpenReferralServiceDto()
            {
                Id = string.IsNullOrEmpty(serviceId) ? Guid.NewGuid().ToString() : serviceId,
                Name = orgService.ServiceName,
                Url = orgService.Website,
                Description = orgService.ServiceDescription,
                Email = orgService.EmailToContactService,
                CanFamilyChooseDeliveryLocation = false,
                Attending_type = orgService.DeliveryMethod,
                Deliverable_type = orgService.DeliveryMethod,
                Status = "active",
                OpenReferralOrganisationId = orgid,
                ServiceType = GetServiceType(orgService.OrgType),
                Contacts = GetContactDetails(orgService),
                Cost_options = GetCostOptions(orgService),
                Eligibilities = GetEligibilities(orgService),
                Service_at_locations = GetServiceAtLocations(orgService),
                Service_taxonomys = GetCategories(orgService),
                ServiceDelivery = string.IsNullOrEmpty(orgService.DeliveryMethod) ? null : GetServiceDeliveries(orgService),
                Languages = string.IsNullOrEmpty(orgService.Language) ? null : GetLanguages(orgService)

            };
        }
        

        private List<OpenReferralServiceTaxonomyDto> GetCategories(StandardData service)
        {          
           

            var categories = new List<OpenReferralServiceTaxonomyDto>();
            if (string.IsNullOrEmpty(service.Category))
            {
                var taxonomy = masterTaxonomies.Where(s => s.Name == service.Category).First();
                categories.Add(new OpenReferralServiceTaxonomyDto(
                    Guid.NewGuid().ToString(), taxonomy));
            }
            //if(!string.IsNullOrEmpty(service.LocationType) && service.LocationType== FamilyHub)
            //{
            //    var taxonomy = masterTaxonomies.Where(s => s.Name == FamilyHub).First();
            //    categories.Add(new OpenReferralServiceTaxonomyDto(
            //        Guid.NewGuid().ToString(), taxonomy));
            //}
            

            return categories;
        }

        private List<OpenReferralServiceDeliveryExDto> GetServiceDeliveries(StandardData service)
        {
            ServiceDelivery serviceDevliveryType;
            Enum.TryParse("Active", out serviceDevliveryType);
            var serviceDeliveries = new List<OpenReferralServiceDeliveryExDto>();
            serviceDeliveries.Add(new OpenReferralServiceDeliveryExDto(id:Guid.NewGuid().ToString(), serviceDelivery : serviceDevliveryType));

            return serviceDeliveries;

        }

        private List<OpenReferralLanguageDto> GetLanguages(StandardData service)
        {
            var openReferralLanguages = new List<OpenReferralLanguageDto>();
            foreach (var language in service.Language.Split(","))
            {
                openReferralLanguages.Add(new OpenReferralLanguageDto(id: Guid.NewGuid().ToString(), language: language));
            }
            return openReferralLanguages;
        }
        

        private static async Task<List<OpenReferralTaxonomyDto>> GetMasterTaxonomy()
        {
            var dfeTaxonomyMasterList = await _apiClient.GetAsync(new Uri(_apiClient.BaseAddress + $"api/taxonomies"));
            var apiResponse = await dfeTaxonomyMasterList.Content.ReadAsStringAsync();
            var t = JsonConvert.DeserializeObject<RootTaxonomyobject>(apiResponse);
            return t.items.ToList();
        }


        private static OrganisationTypeDto GetOrganisationType(string type)
        {
            switch(type)
            {
                case "Local Authority": return new OrganisationTypeDto("1", "LA", "Local Authority"); 
                case "Voluntary and Community Sector": return
                    new OrganisationTypeDto("2", "VCFS", "Voluntary, Charitable, Faith Sector"); 
            case "Public / Private Company eg: Child Care":
                    return new OrganisationTypeDto("3", "Company", "Public / Private Company eg: Child Care Centre");
                case "Family Hub":
                    return new OrganisationTypeDto("4", "FamilyHub", "Family Hubs");
            }
            return null;
        }

        // get the confirmation
        private ServiceTypeDto GetServiceType(string type)
        {
            // return new ServiceTypeDto(id: "2", name: "FX", description: "Family Experience");
            return (type == "Local Authority" || type== "Family Hub")  ? new ServiceTypeDto(id: "2", name: "FX", description: "Family Experience")
            : new ServiceTypeDto(id: "1", name: "IS", description: "Information Sharing");
        }

        private List<OpenReferralPhoneDto> GetContactNumbers(StandardData service)
        {
            List<OpenReferralPhoneDto> contactNumbers = new();

            contactNumbers.Add(new OpenReferralPhoneDto() { Number = service.PhoneToContactService, Id = Guid.NewGuid().ToString() });
            //if (!string.IsNullOrEmpty(service.TextToContactService)) contactNumbers.Add(new OpenReferralPhoneDto() { Number = service.TextToContactService, Id = Guid.NewGuid().ToString() });
            return contactNumbers;
        }


        private List<OpenReferralContactDto> GetContactDetails(StandardData service)
        {
            var ContactDetails = new List<OpenReferralContactDto>();
            ContactDetails.Add(new OpenReferralContactDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Telephone",
                Phones = string.IsNullOrEmpty(service.PhoneToContactService)? null: GetContactNumbers(service)
            });

            if (!string.IsNullOrEmpty(service.TextToContactService))
            {
                ContactDetails.Add(new OpenReferralContactDto()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Text Phone",
                    Phones = new List<OpenReferralPhoneDto>() { new OpenReferralPhoneDto()

                     { Number = service.TextToContactService, Id = Guid.NewGuid().ToString() }
                    }
                });                
            }            

            return ContactDetails;
        }

        private OpenReferralCostOptionDto GetCost(string providercost, string option)
        {
            providercost = providercost.Contains('£') ? providercost.Remove(0,1) : providercost;
            decimal costDecimal;
            return new OpenReferralCostOptionDto()
            {
                Id = Guid.NewGuid().ToString(),
                Amount = (decimal.TryParse(providercost, out costDecimal) ? costDecimal : 0),
                Amount_description = option
            };
        }

        private List<OpenReferralCostOptionDto> GetCostOptions(StandardData service)
        {
            var CostOptions = new List<OpenReferralCostOptionDto>();

            if (service.Cost != "Yes" || string.IsNullOrEmpty(service.Cost))
            {
                CostOptions.Add(GetCost("0", "Free"));
            }
            else
            {
                if (!string.IsNullOrEmpty(service.CostPerUnit)) { CostOptions.Add(GetCost(service.CostInPounds
                    ,service.CostPerUnit)); }
            }          

            return CostOptions;

        }


        private List<OpenReferralEligibilityDto> GetEligibilities(StandardData service)
        {
            if (string.IsNullOrEmpty(service.MinAge) && string.IsNullOrEmpty(service.MaxAge)) return null;


           // service.MinAge = service.MinAge //service.MinAge.IndexOf("years")>0 ? service.MinAge.Substring(0,service.MinAge.IndexOf("years")) : service.MinAge.Substring(0, service.MinAge.IndexOf("year"));
            //service.MaxAge = //service.MaxAge.IndexOf("years") > 0 ? service.MaxAge.Substring(0, service.MaxAge.IndexOf("years")) : service.MaxAge.Substring(0, service.MaxAge.IndexOf("year"));

            var eligibilities = new List<OpenReferralEligibilityDto>();
            eligibilities.Add(new OpenReferralEligibilityDto(id: Guid.NewGuid().ToString(), eligibility: "child",
                maximum_age: Convert.ToInt32(service.MaxAge), minimum_age: Convert.ToInt32(service.MinAge))
            );

            return eligibilities;

        }

        private List<OpenReferralServiceAtLocationDto> GetServiceAtLocations(StandardData service)
        {

            var serviceAtLocations = new List<OpenReferralServiceAtLocationDto>();
            serviceAtLocations.Add(new OpenReferralServiceAtLocationDto(id: Guid.NewGuid().ToString(),location: GetLocations(service),regular_schedule:GetSchedules(service),holidayScheduleCollection:null));
            return serviceAtLocations;
        }


        private OpenReferralLocationDto GetLocations(StandardData  service)
        {
            var longlatDetails =_longitudeLatitudeFinder.GetLongitudeLatitudeForPostcode(service.Postcode, _postcodeLocationService);
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

        private List<OpenReferralRegularScheduleDto> GetSchedules(StandardData service)
        {
            
            var schedules = new List<OpenReferralRegularScheduleDto>();  

            

                schedules.Add(new OpenReferralRegularScheduleDto(Guid.NewGuid().ToString(),
                    description: service.OpeningHoursDescription,
                    opens_at: null,
                    closes_at: null,
                    byday: null,
                    bymonthday: null,
                    dtstart: null,
                    freq: null,
                    interval: null,
                    valid_from: null,
                    valid_to: null
                    ));
            
            
            return schedules;
        }


        private List<OpenReferralPhysicalAddressDto> GetPhysicalAddress(StandardData service)
        {
            var PhysicalAddresses = new List<OpenReferralPhysicalAddressDto>();

            //CheckIfLocationExists(service.Postcode);

            PhysicalAddresses.Add(new OpenReferralPhysicalAddressDto(Guid.NewGuid().ToString(), service.AddressLine1, service.TownorCity, service.Postcode, "UK", service.County)
            );
            return PhysicalAddresses;

        }

        private async Task<List<OpenReferralOrganisationDto>> GetOrganisations()
        { 
            using var response = await _apiClient.GetAsync(new Uri(_apiClient.BaseAddress + $"api/organizations"));
            var apiResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<OpenReferralOrganisationDto>>(apiResponse);
        }

        private OpenReferralOrganisationDto GetOrganisationInfo(string organisationName)
        {         
            return _masterOrgs.Where(k=>k.Name==organisationName).FirstOrDefault();
        }

        private async Task<OpenReferralOrganisationWithServicesDto> GetServicesForOrganisation(string organisationId)
        {
            using var response = await _apiClient.GetAsync(new Uri(_apiClient.BaseAddress + $"api/organizations/{organisationId}"));
            var apiResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OpenReferralOrganisationWithServicesDto>(apiResponse);
        }

    }
}
