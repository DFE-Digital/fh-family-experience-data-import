using System.Text;
using FamilyExperience.Dataimport.Models;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralContacts;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralCostOptions;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralEligibilitys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLanguages;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhones;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhysicalAddresses;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralRegularSchedule;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceAtLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceDeliverysEx;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceTaxonomys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OrganisationType;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.ServiceType;
using Newtonsoft.Json;

namespace FamilyExperience.Dataimport.Service
{
    public class TransformDataService
    {
        private readonly PostcodeLocationService _postcodeLocationService;
        private List<OpenReferralTaxonomyDto> _masterTaxonomies;
        private List<OpenReferralOrganisationDto> _masterOrgs;
        private OpenReferralOrganisationWithServicesDto _openReferralOrganisationWithServicesDtos;

        private const string LocalAuthority = "Local Authority";
        private const string FamilyHub = "Family Hub";
        private const string Vcs = "Voluntary and Community Sector";

        private const string StandardDataExcelPath = @"C:\DATA\Downloads\Local Authority Data Capture v4.0.xlsm";
        private const string ServiceDirectoryApiBaseAddress = "https://s181d01-as-fh-sd-api-dev.azurewebsites.net/";
        //private const string ServiceDirectoryApiBaseAddress = "https://localhost:7022/";

        private static readonly HttpClient ApiClient = new HttpClient
        {
            BaseAddress = new Uri(ServiceDirectoryApiBaseAddress)
        };

        public TransformDataService()
        {
            _postcodeLocationService = new PostcodeLocationService();
            _masterTaxonomies = new List<OpenReferralTaxonomyDto>();
        }

        public async Task ProcessDataAsync()
        {
            try
            {
                _masterTaxonomies = await GetMasterTaxonomy();
                _masterOrgs = await GetOrganisations();

                var standardData = ExcelReaderService.ReadExcel<List<StandardData>>(StandardDataExcelPath) ?? new List<StandardData>();
                
                foreach (var dataRow in standardData)
                {
                    var openReferralOrganisationDto = GetOrganisationInfo(dataRow.OrganisationName);

                    if (dataRow.OrgType is FamilyHub or Vcs && openReferralOrganisationDto == null)
                    {
                        var openReferralOrgRecord = new OpenReferralOrganisationWithServicesDto
                        {
                            Name = dataRow.OrganisationName,
                            Id = Guid.NewGuid().ToString(),
                            OrganisationType = GetOrganisationType(dataRow.OrgType),
                            AdministractiveDistrictCode = _postcodeLocationService.GetLongitudeLatitudeForPostcode(dataRow.Postcode).AdministrativeDistrictCode,
                            Url = dataRow.Website,
                            Services = null
                        };

                        var apiRequest = new HttpRequestMessage
                        {
                            Method = HttpMethod.Post,
                            RequestUri = new Uri(ApiClient.BaseAddress + "api/organizations"),
                            Content = new StringContent(JsonConvert.SerializeObject(openReferralOrgRecord), Encoding.UTF8, "application/json")
                        };

                        var response = await ApiClient.SendAsync(apiRequest);

                        Console.WriteLine(response.StatusCode);

                        _masterOrgs = await GetOrganisations();
                    }

                    openReferralOrganisationDto = GetOrganisationInfo(dataRow.OrganisationName);

                    _openReferralOrganisationWithServicesDtos = await GetServicesForOrganisation(openReferralOrganisationDto.Id);

                    var existingService = _openReferralOrganisationWithServicesDtos.Services?.FirstOrDefault(x => x.Name == dataRow.ServiceName);

                    if (existingService != null && !string.IsNullOrEmpty(existingService.Id))
                    {
                        await UpdateService(existingService);
                    }
                    else
                    {
                        var service = CreateOpenReferralServiceDto(dataRow, openReferralOrganisationDto.Id, null);

                        var apiRequest = new HttpRequestMessage
                        {
                            Method = HttpMethod.Post,
                            RequestUri = new Uri(ApiClient.BaseAddress + "api/services"),
                            Content = new StringContent(JsonConvert.SerializeObject(service), Encoding.UTF8, "application/json")
                        };

                        var response = await ApiClient.SendAsync(apiRequest);

                        Console.WriteLine(response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        private static async Task UpdateService(IOpenReferralServiceDto service)
        {
            try
            {
                var apiRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(ApiClient.BaseAddress + $"api/services/{service.Id}"),
                    Content = new StringContent(JsonConvert.SerializeObject(service), Encoding.UTF8, "application/json")
                };

                var response = await ApiClient.SendAsync(apiRequest);

                Console.WriteLine(response.StatusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private OpenReferralServiceDto CreateOpenReferralServiceDto(StandardData orgService, string orgid, string serviceId)
        {
            return new OpenReferralServiceDto
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
                Eligibilities = GetEligibility(orgService),
                Service_at_locations = GetServiceAtLocations(orgService),
                Service_taxonomys = GetCategories(orgService),
                ServiceDelivery = string.IsNullOrEmpty(orgService.DeliveryMethod) ? null : GetServiceDeliveries(),
                Languages = string.IsNullOrEmpty(orgService.Language) ? null : GetLanguages(orgService)
            };
        }

        private List<OpenReferralServiceTaxonomyDto> GetCategories(StandardData service)
        {
            var categories = new List<OpenReferralServiceTaxonomyDto>();

            if (!string.IsNullOrEmpty(service.Category)) return categories;

            var taxonomy = _masterTaxonomies.First(s => s.Name == service.Category);

            categories.Add(new OpenReferralServiceTaxonomyDto(Guid.NewGuid().ToString(), taxonomy));

            return categories;
        }

        private static List<OpenReferralServiceDeliveryExDto> GetServiceDeliveries()
        {
            Enum.TryParse("Active", out ServiceDelivery serviceDeliveryType);

            var serviceDeliveries = new List<OpenReferralServiceDeliveryExDto> { new OpenReferralServiceDeliveryExDto(Guid.NewGuid().ToString(), serviceDeliveryType) };

            return serviceDeliveries;
        }

        private static List<OpenReferralLanguageDto> GetLanguages(StandardData service)
        {
            return service.Language
                .Split(",")
                .Select(language => new OpenReferralLanguageDto(Guid.NewGuid().ToString(), language))
                .ToList();
        }

        private static async Task<List<OpenReferralTaxonomyDto>> GetMasterTaxonomy()
        {
            var taxonomyMasterList = await ApiClient.GetAsync(new Uri(ApiClient.BaseAddress + "api/taxonomies"));

            var apiResponse = await taxonomyMasterList.Content.ReadAsStringAsync();

            var taxonomyObject = JsonConvert.DeserializeObject<RootTaxonomyObject>(apiResponse);

            return taxonomyObject?.Items.ToList() ?? new List<OpenReferralTaxonomyDto>();
        }

        private static OrganisationTypeDto GetOrganisationType(string type)
        {
            return type switch
            {
                LocalAuthority => new OrganisationTypeDto("1", "LA", "Local Authority"),
                Vcs => new OrganisationTypeDto("2", "VCFS", "Voluntary, Charitable, Faith Sector"),
                FamilyHub => new OrganisationTypeDto("4", "FamilyHub", "Family Hubs"),
                "Public / Private Company eg: Child Care" => new OrganisationTypeDto("3", "Company", "Public / Private Company eg: Child Care Centre"),
                _ => null
            };
        }

        // get the confirmation
        private static ServiceTypeDto GetServiceType(string type)
        {
            return type is LocalAuthority or FamilyHub
                ? new ServiceTypeDto("2", "FX", "Family Experience")
                : new ServiceTypeDto("1", "IS", "Information Sharing");
        }

        private static List<OpenReferralPhoneDto> GetContactNumbers(StandardData service)
        {
            List<OpenReferralPhoneDto> contactNumbers = new() { new OpenReferralPhoneDto { Number = service.PhoneToContactService, Id = Guid.NewGuid().ToString() } };

            return contactNumbers;
        }

        private static List<OpenReferralContactDto> GetContactDetails(StandardData service)
        {
            var contactDetails = new List<OpenReferralContactDto>
            {
                new OpenReferralContactDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Telephone",
                    Phones = !string.IsNullOrEmpty(service.PhoneToContactService)
                        ? GetContactNumbers(service)
                        : null
                }
            };

            if (!string.IsNullOrEmpty(service.TextToContactService))
            {
                contactDetails.Add(new OpenReferralContactDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Text Phone",
                    Phones = new List<OpenReferralPhoneDto>
                    {
                        new OpenReferralPhoneDto
                        {
                            Number = service.TextToContactService,
                            Id = Guid.NewGuid().ToString()
                        }
                    }
                });
            }

            return contactDetails;
        }

        private static OpenReferralCostOptionDto GetCost(string providerCost, string option)
        {
            providerCost = providerCost.Contains('£')
                ? providerCost.Remove(0, 1)
                : providerCost;

            return new OpenReferralCostOptionDto
            {
                Id = Guid.NewGuid().ToString(),
                Amount = decimal.TryParse(providerCost, out var costDecimal) ? costDecimal : 0,
                Amount_description = option
            };
        }

        private static List<OpenReferralCostOptionDto> GetCostOptions(StandardData service)
        {
            var costOptions = new List<OpenReferralCostOptionDto>();

            if (service.Cost != "Yes" || string.IsNullOrEmpty(service.Cost))
            {
                costOptions.Add(GetCost("0", "Free"));
            }
            else
            {
                if (!string.IsNullOrEmpty(service.CostPerUnit))
                {
                    costOptions.Add(GetCost(service.CostInPounds, service.CostPerUnit));
                }
            }

            return costOptions;
        }

        private static List<OpenReferralEligibilityDto> GetEligibility(StandardData service)
        {
            if (string.IsNullOrEmpty(service.MinAge) && string.IsNullOrEmpty(service.MaxAge)) return null;

            var eligibility = new List<OpenReferralEligibilityDto>
            {
                new OpenReferralEligibilityDto(Guid.NewGuid().ToString(), "child", Convert.ToInt32(service.MaxAge), Convert.ToInt32(service.MinAge))
            };

            return eligibility;
        }

        private List<OpenReferralServiceAtLocationDto> GetServiceAtLocations(StandardData service)
        {

            var serviceAtLocations = new List<OpenReferralServiceAtLocationDto>
            {
                new OpenReferralServiceAtLocationDto(Guid.NewGuid().ToString(),GetLocations(service),GetSchedules(service),null)
            };

            return serviceAtLocations;
        }

        private OpenReferralLocationDto GetLocations(StandardData service)
        {
            var longitudeLatitudeForPostcode = _postcodeLocationService.GetLongitudeLatitudeForPostcode(service.Postcode);

            return new OpenReferralLocationDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = service.LocationName,
                Description = service.LocationName,
                Latitude = longitudeLatitudeForPostcode.Latitude,
                Longitude = longitudeLatitudeForPostcode.Longitude,
                Physical_addresses = GetPhysicalAddress(service)
            };
        }

        private List<OpenReferralRegularScheduleDto> GetSchedules(StandardData service)
        {
            var schedules = new List<OpenReferralRegularScheduleDto>
            {
                new OpenReferralRegularScheduleDto(Guid.NewGuid().ToString(),
                    service.OpeningHoursDescription,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null
                )
            };

            return schedules;
        }

        private static List<OpenReferralPhysicalAddressDto> GetPhysicalAddress(StandardData service)
        {
            var physicalAddresses = new List<OpenReferralPhysicalAddressDto>
            {
                new OpenReferralPhysicalAddressDto(Guid.NewGuid().ToString(), service.AddressLine1, service.TownOrCity, service.Postcode, "UK", service.County)
            };

            return physicalAddresses;
        }

        private static async Task<List<OpenReferralOrganisationDto>> GetOrganisations()
        {
            var response = await ApiClient.GetAsync(new Uri(ApiClient.BaseAddress + "api/organizations"));
            
            var apiResponse = await response.Content.ReadAsStringAsync();
            
            var organisations = JsonConvert.DeserializeObject<List<OpenReferralOrganisationDto>>(apiResponse);

            return organisations;
        }

        private OpenReferralOrganisationDto GetOrganisationInfo(string organisationName)
        {
            return _masterOrgs.FirstOrDefault(k => k.Name == organisationName);
        }

        private static async Task<OpenReferralOrganisationWithServicesDto> GetServicesForOrganisation(string organisationId)
        {
            var response = await ApiClient.GetAsync(new Uri(ApiClient.BaseAddress + $"api/organizations/{organisationId}"));
            
            var apiResponse = await response.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<OpenReferralOrganisationWithServicesDto>(apiResponse);
        }
    }
}