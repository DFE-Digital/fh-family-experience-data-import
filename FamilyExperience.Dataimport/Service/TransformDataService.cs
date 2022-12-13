using System.Text;
using FamilyExperience.DataImport.Models;
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

namespace FamilyExperience.DataImport.Service
{
    public class TransformDataService
    {
        private readonly PostcodeLocationService _postcodeLocationService;
        private List<OpenReferralTaxonomyDto> _masterTaxonomies;
        private List<OpenReferralOrganisationDto> _masterOrgs;
        private OpenReferralOrganisationWithServicesDto _openReferralOrganisationWithServicesDtos;
        private OpenReferralServiceDto _existsingServiceDto;

        private const string LocalAuthority = "Local Authority";
        private const string FamilyHub = "Family Hub";
        private const string Vcs = "Voluntary and Community Sector";
        private const string Vcfs = "Voluntary, Charitable, Faith Sector";
        private const string Company = "Company";
        private const string Telephone = "Telephone";
        private const string TextPhone = "Text Phone";
        private bool IsExistingService = false;

        private const string StandardDataExcelPath = @"D:\DFE\Local Authority Data Capture v6.1 After School Clubs.xlsm";
        //private const string ServiceDirectoryApiBaseAddress = "https://s181d01-as-fh-sd-api-dev.azurewebsites.net/";
        private const string ServiceDirectoryApiBaseAddress = "https://localhost:7022/";

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

                Console.WriteLine($"Process started to call Service directory API.");

                foreach (var dataRow in standardData)
                {
                    try
                    {
                        _existsingServiceDto = new OpenReferralServiceDto();
                        IsExistingService = false;
                        var openReferralOrganisationDto = GetOrganisationInfo(string.IsNullOrEmpty(dataRow.OrganisationName) ? dataRow.LAName : dataRow.OrganisationName, dataRow.OrgType);
                        var adminDistrictCode = _postcodeLocationService.GetLongitudeLatitudeForPostcode(dataRow.Postcode).AdministrativeDistrictCode;

                        if (openReferralOrganisationDto == null)
                        {
                            var openReferralOrgRecord = new OpenReferralOrganisationWithServicesDto
                            {
                                Name = dataRow.OrganisationName,
                                Id = Guid.NewGuid().ToString(),
                                OrganisationType = GetOrganisationType(dataRow.OrgType),
                                AdministractiveDistrictCode = adminDistrictCode,
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

                        openReferralOrganisationDto = GetOrganisationInfo(string.IsNullOrEmpty(dataRow.OrganisationName) ?  dataRow.LAName : dataRow.OrganisationName, dataRow.OrgType);

                        _openReferralOrganisationWithServicesDtos = await GetServicesForOrganisation(openReferralOrganisationDto.Id);

                        //var existingService = _openReferralOrganisationWithServicesDtos.Services?.FirstOrDefault(x => x.Id == dataRow.ServiceId && x.OpenReferralOrganisationId == openReferralOrganisationDto.Id);
                        _existsingServiceDto = _openReferralOrganisationWithServicesDtos.Services?.FirstOrDefault(x => x.Name == dataRow.ServiceName
                        && x.OpenReferralOrganisationId == openReferralOrganisationDto.Id);


                        if (_existsingServiceDto != null && !string.IsNullOrEmpty(_existsingServiceDto.Id))
                        {
                            IsExistingService = true;
                            Console.WriteLine($"Updating the service: {_existsingServiceDto.Name} started.");
                            var service = CreateOpenReferralServiceDto(dataRow, openReferralOrganisationDto.Id, _existsingServiceDto.Id, adminDistrictCode);
                            await UpdateService(service);
                            Console.WriteLine($"Updating the service:{_existsingServiceDto.Name} completed.");
                        }
                        else
                        {
                            Console.WriteLine($"Creating the service:{dataRow.ServiceName} through API started.");
                            var service = CreateOpenReferralServiceDto(dataRow, openReferralOrganisationDto.Id, null, adminDistrictCode);

                            var apiRequest = new HttpRequestMessage
                            {
                                Method = HttpMethod.Post,
                                RequestUri = new Uri(ApiClient.BaseAddress + "api/services"),
                                Content = new StringContent(JsonConvert.SerializeObject(service), Encoding.UTF8, "application/json")
                            };

                            var response = await ApiClient.SendAsync(apiRequest);

                            Console.WriteLine(response.IsSuccessStatusCode ? $"Creating the service:{dataRow.ServiceName} through API completed." : $"some error occured while creating the service:{dataRow.ServiceName} through API,{response.StatusCode}.");
                        }
                    }
                    catch (Exception ex)
                    { continue; throw new Exception($"Some error occured on service :{dataRow.ServiceName}, {ex.Message}"); }
                }    
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            Console.WriteLine($"Process completed.");
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

                Console.WriteLine(response.IsSuccessStatusCode? $"updating the service: {service.Name} completed successfully." : $"Error occured while updating the service: {service.Name},{response.StatusCode}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private OpenReferralServiceDto CreateOpenReferralServiceDto(StandardData orgService, string orgid, string serviceId,string adminDistrictCode)
        {
           return new OpenReferralServiceDto
            {
                Id = string.IsNullOrEmpty(serviceId) ? GeneratedServiceId(orgService) : serviceId,
                Name = orgService.ServiceName,
                Url = orgService.Website,
                Description = orgService.ServiceDescription,
                Email = orgService.ContactEmail,
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


        private string GeneratedServiceId(StandardData dataRow)
        {
            var openReferralOrganisationDto = GetOrganisationInfo(string.IsNullOrEmpty(dataRow.OrganisationName) ? dataRow.LAName : dataRow.OrganisationName, dataRow.OrgType);
            return String.IsNullOrEmpty(openReferralOrganisationDto.AdministractiveDistrictCode) ? Guid.NewGuid().ToString() : $"{openReferralOrganisationDto.AdministractiveDistrictCode.Remove(0, 1)}{dataRow.ServiceId}";
        
        }

        private List<OpenReferralServiceTaxonomyDto> GetCategories(StandardData service)
        {
            var categories = new List<OpenReferralServiceTaxonomyDto>();

            if (string.IsNullOrEmpty(service.Category)) return categories;

            var taxonomy = _masterTaxonomies.First(s => s.Name == service.Category);

            categories.Add(new OpenReferralServiceTaxonomyDto(IsExistingService ?(_existsingServiceDto.Service_taxonomys.Count>0 ? _existsingServiceDto.Service_taxonomys.First().Id :Guid.NewGuid().ToString()) : Guid.NewGuid().ToString(), taxonomy));

            return categories;
        }

        private List<OpenReferralServiceDeliveryExDto> GetServiceDeliveries()
        {
            Enum.TryParse("Active", out ServiceDelivery serviceDeliveryType);

            var serviceDeliveries = new List<OpenReferralServiceDeliveryExDto> { new OpenReferralServiceDeliveryExDto(IsExistingService ? (_existsingServiceDto.ServiceDelivery.Count >0? _existsingServiceDto.ServiceDelivery.First().Id : Guid.NewGuid().ToString()): Guid.NewGuid().ToString(), serviceDeliveryType) };

            return serviceDeliveries;
        }


        private List<OpenReferralLanguageDto> GetLanguages(StandardData service)
        {

            var languages = service.Language.Split(" | ");
            var languageList = new List<OpenReferralLanguageDto>();
            if (IsExistingService)
            {
                foreach (var updatedLanguage in languages)
                {
                    var current = _existsingServiceDto.Languages.FirstOrDefault(x => x.Language == updatedLanguage);
                    if (current == null)
                    {
                        languageList.Add(new OpenReferralLanguageDto(Guid.NewGuid().ToString(), updatedLanguage));
                    }
                    else
                    {
                        languageList.Add(new OpenReferralLanguageDto(current.Id, updatedLanguage));

                    }
                }
                return languageList;
            }

            return service.Language
                .Split(" | ")
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
                FamilyHub => new OrganisationTypeDto("3", "FamilyHub", "Family Hub"),
                 Company => new OrganisationTypeDto("4", "Company", "Public / Private Company eg: Child Care Centre"),
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

        private List<OpenReferralPhoneDto> GetContactNumbers(StandardData service)
        {
            List<OpenReferralPhoneDto> contactNumbers = new() { 
                new OpenReferralPhoneDto { Number = service.ContactPhone, Id = IsExistingService? (_existsingServiceDto.Contacts.Count(x => x.Name == Telephone)>0 ? _existsingServiceDto.Contacts.Where(x=>x.Name==Telephone).FirstOrDefault().Phones.FirstOrDefault().Id : Guid.NewGuid().ToString()) :  Guid.NewGuid().ToString() } };

            return contactNumbers;
        }

        private List<OpenReferralContactDto> GetContactDetails(StandardData service)
        {
            var contactDetails = new List<OpenReferralContactDto>();

            if(!string.IsNullOrEmpty(service.ContactPhone) )
            {
                contactDetails.Add(new OpenReferralContactDto
                {
                    Id = IsExistingService ? (_existsingServiceDto.Contacts.Count(x => x.Name == Telephone)>0 ? _existsingServiceDto.Contacts.Where(x=>x.Name==Telephone).FirstOrDefault().Id :Guid.NewGuid().ToString()) : Guid.NewGuid().ToString(),
                    Name = Telephone,
                    Phones = GetContactNumbers(service)

                });
            }

            if (!string.IsNullOrEmpty(service.TextToContactService))
            {
                contactDetails.Add(new OpenReferralContactDto
                {
                    Id = IsExistingService ? (_existsingServiceDto.Contacts.Count(x => x.Name == TextPhone) > 0 ? _existsingServiceDto.Contacts.Where(x => x.Name == TextPhone).FirstOrDefault().Id : Guid.NewGuid().ToString()):  Guid.NewGuid().ToString(),
                    Name = TextPhone,
                    Phones = new List<OpenReferralPhoneDto>
                    {
                        new OpenReferralPhoneDto
                        {
                            Number = service.TextToContactService,
                            Id = IsExistingService? (_existsingServiceDto.Contacts.Count(x => x.Name == TextPhone) > 0 ? _existsingServiceDto.Contacts.Where(x => x.Name == TextPhone).FirstOrDefault().Id : Guid.NewGuid().ToString()): Guid.NewGuid().ToString()
                        }
                    }
                });
            }

            return contactDetails;
        }

        private OpenReferralCostOptionDto GetCost(string providerCost, string option,string costDescription)
        {
            providerCost = providerCost.Contains('£')
                ? providerCost.Remove(0, 1)
                : providerCost;

            return new OpenReferralCostOptionDto
            {
                Id = IsExistingService ? _existsingServiceDto.Cost_options.First().Id  :Guid.NewGuid().ToString(),
                Amount = decimal.TryParse(providerCost, out var costDecimal) ? costDecimal : 0,
                Option = option,
                Amount_description = costDescription
            };
        }

        private List<OpenReferralCostOptionDto> GetCostOptions(StandardData service)
        {
            var costOptions = new List<OpenReferralCostOptionDto>();
                     
                if ((!string.IsNullOrEmpty(service.CostPerUnit) && !string.IsNullOrEmpty(service.CostInPounds)) || (!string.IsNullOrEmpty(service.CostDescription) ))
                {
                    costOptions.Add(GetCost(service.CostInPounds, service.CostPerUnit,service.CostDescription));
                }          

            return costOptions;
        }

        private List<OpenReferralEligibilityDto> GetEligibility(StandardData service)
        {           

            try {
                if (string.IsNullOrEmpty(service.MinAge) ||string.IsNullOrEmpty(service.MaxAge)) return null;
                service.MaxAge=  service.MaxAge == "25+" ? "127" : service.MaxAge;               

                var eligibility = new List<OpenReferralEligibilityDto>
                {
                    new OpenReferralEligibilityDto(IsExistingService ? _existsingServiceDto.Eligibilities.Count > 0 ?
                    _existsingServiceDto.Eligibilities.First().Id : Guid.NewGuid().ToString() : Guid.NewGuid().ToString(), "child", Convert.ToInt32(service.MaxAge), Convert.ToInt32(service.MinAge))
                };
                return eligibility;

            } catch (Exception ex) { Console.WriteLine($"Error occured at service :{ex.Message}."); }
            return new List<OpenReferralEligibilityDto>();
        }
               

        private List<OpenReferralServiceAtLocationDto> GetServiceAtLocations(StandardData service)
        {

            var serviceAtLocations = new List<OpenReferralServiceAtLocationDto>
            {
                new OpenReferralServiceAtLocationDto(IsExistingService ? _existsingServiceDto.Service_at_locations.Count> 0 ? _existsingServiceDto.Service_at_locations.First().Id: Guid.NewGuid().ToString(): Guid.NewGuid().ToString() ,GetLocations(service),GetSchedules(service),null)
            };

            return serviceAtLocations;
        }

        private OpenReferralLocationDto GetLocations(StandardData service)
        {
            var longitudeLatitudeForPostcode = _postcodeLocationService.GetLongitudeLatitudeForPostcode(service.Postcode);

            return new OpenReferralLocationDto
            {
                Id = IsExistingService? _existsingServiceDto.Service_at_locations.Count >0 ? _existsingServiceDto.Service_at_locations.First().Location.Id  :Guid.NewGuid().ToString() : Guid.NewGuid().ToString(),
                Name = service.LocationName,
                Description = service.LocationDescription,
                Latitude = longitudeLatitudeForPostcode.Latitude,
                Longitude = longitudeLatitudeForPostcode.Longitude,
                Physical_addresses = GetPhysicalAddress(service)
            };
        }

        private List<OpenReferralRegularScheduleDto> GetSchedules(StandardData service)
        {
            var schedules = new List<OpenReferralRegularScheduleDto>
            {
                new OpenReferralRegularScheduleDto(IsExistingService? (_existsingServiceDto.Service_at_locations.FirstOrDefault().Regular_schedule.Count >0 ? _existsingServiceDto.Service_at_locations.First().Regular_schedule.First().Id : Guid.NewGuid().ToString()):Guid.NewGuid().ToString() ,
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
        

        private  List<OpenReferralPhysicalAddressDto> GetPhysicalAddress(StandardData service)
        {
            string address = string.IsNullOrEmpty(service.AddressLine2) ? $"{service.AddressLine1},{service.AddressLine2}" : service.AddressLine1;
            var physicalAddresses = new List<OpenReferralPhysicalAddressDto>
            {
                new OpenReferralPhysicalAddressDto(IsExistingService? _existsingServiceDto.Service_at_locations.First().Location.Physical_addresses.Count >0 ? _existsingServiceDto.Service_at_locations.First().Location.Physical_addresses.First().Id :Guid.NewGuid().ToString():Guid.NewGuid().ToString(), address, service.TownOrCity, service.Postcode, "UK", service.County)
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

        private OpenReferralOrganisationDto GetOrganisationInfo(string organisationName, string organisationType)
        {
            if (organisationType == Vcs) organisationType = Vcfs;
            return _masterOrgs.FirstOrDefault(k => k.Name == organisationName && k.OrganisationType.Description == organisationType);
        }

        private static async Task<OpenReferralOrganisationWithServicesDto> GetServicesForOrganisation(string organisationId)
        {
            var response = await ApiClient.GetAsync(new Uri(ApiClient.BaseAddress + $"api/organizations/{organisationId}"));
            
            var apiResponse = await response.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<OpenReferralOrganisationWithServicesDto>(apiResponse);
        }
    }
}