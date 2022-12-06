using System.Text;
using AutoMapper;
using FamilyExperience.Dataimport.Mapping;
using FamilyExperience.Dataimport.Models;
using FamilyExperience.Dataimport.Models.API;
using Newtonsoft.Json;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OrganisationType;
using FamilyExperience.Dataimport.Service;

namespace FamilyExperience.Dataimport
{
    public class TransformSalfordData
    {
        protected static HttpClient _client;
        protected static HttpClient? _apiClient;
        protected PostcodeLocationService _postcodeLocationService;
        private List<LongitudeLatitude> longitudeLatitudes;
        private Rootobject servicesList { get;set;}


        public TransformSalfordData()
        {
            _postcodeLocationService = new PostcodeLocationService();
            longitudeLatitudes = new List<LongitudeLatitude>();
        }
        
        public async Task ProcessDataAsync()
        {
            var orgId = "ca8ddaeb-b5e5-46c4-b94d-43a8e2ccc066";


            try
            {
                servicesList = await GetSalfordDataAsync();
                var mapper = GetMapper();

                var openReferralOrgRecord = new OpenReferralOrganisationWithServicesDto() { Name = "Salford City Council", Url = "https://www.salford.gov.uk/", Description = "Salford City Council" };
                openReferralOrgRecord.Id = orgId; // Guid.NewGuid().ToString();
                openReferralOrgRecord.OrganisationType = GetOrganisationType();

                foreach (var service in servicesList.records)
                {
                    service.OrganisationId = orgId; //openReferralOrgRecord.Id;
                    service.ServiceType = GetServiceType();
                    service.Id = Guid.NewGuid().ToString();
                    //service.LastUpdate = Convert.ToDateTime(service.LastUpdate).ToUniversalTime();
                    service.description = service.description.Substring(0, service.description.Length > 500 ? 500 : service.description.Length); // change this 

                    // contacts
                   service.ContactDetails =  GetContactDetails(service);

                    // cost option
                    service.CostOptions = GetCostOptions(service);

                    //location
                    service.ServiceAtLocations = GetServiceAtLocations(service);

                    
                    

                    openReferralOrgRecord.AdministractiveDistrictCode = GetLongitudeLatitudeForPostcode(service.Postcode).AdministractiveDistrictCode;

                    var regularSchedules = new List<RegularSchedule>();

                    // Regular and Holiday schedule, Agerange needs to be implemented

                    //foreach (var session in item.DatesessionInfo)
                    //{
                    //    //DateTime d;
                    //    regularSchedules.Add(new RegularSchedule()
                    //    {
                    //        Id = Guid.NewGuid().ToString(),
                    //        // OpensAt =  IsValidTime(session.Substring(0, session.IndexOf("to")).ToString(),out d) ? d:,
                    //        //ClosesAt = Convert.ToDateTime(session.Substring(session.IndexOf("to")+1).ToString())

                    //    });

                    //}                  

                }

                var openReferralRecord = mapper.Map<List<OpenReferralServiceDto>>(servicesList.records);
                openReferralOrgRecord.Services = openReferralRecord;


                //var test = new StringContent(JsonConvert.SerializeObject(openReferralOrgRecord));
                //new Uri("https://localhost:7022/")  

                _apiClient = new HttpClient
                {
                    BaseAddress = new Uri("https://s181d01-as-fh-sd-api-dev.azurewebsites.net/")

                };
                var apiRequest = new HttpRequestMessage() {
                    Method = HttpMethod.Put, 
                    RequestUri = new Uri(_apiClient.BaseAddress + $"api/organizations/{orgId}"),
                    Content = new StringContent(JsonConvert.SerializeObject(openReferralOrgRecord),
                    Encoding.UTF8, "application/json") };
                using var isapiResponse = await _apiClient.SendAsync(apiRequest);
               
                Console.WriteLine(isapiResponse.StatusCode);
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); Console.ReadLine(); }

        }

        private List<ContactDetails> GetContactDetails(Record service)
        {
            var ContactDetails = new List<ContactDetails>();
            ContactDetails.Add(new ContactDetails()
            {
                Id = Guid.NewGuid().ToString(),
                Name = string.IsNullOrEmpty(service.ContactName) ? service.Name: service.ContactName,
                Numbers = GetContactNumbers(service.ContactTelephone)
            });

            return ContactDetails;
        }


        private void CheckIfLocationExists(string postcode)
        {
            if (servicesList.records.Count(x => x.ServiceAtLocations is not null) > 0)
            {
                var itemss = servicesList.records
                .Where(s => s.ServiceAtLocations.Any(st => st.LocationDetails.PhysicalAddresses.Any(si => si.Postal_code == postcode)))
                 .ToList();
            }



          
        }


        private List<ServiceAtLocation> GetServiceAtLocations(Record service)
        {

            var serviceAtLocations = new List<ServiceAtLocation>();
            serviceAtLocations.Add(new ServiceAtLocation()
            {
                Id = Guid.NewGuid().ToString(),
                LocationDetails = GetLocations(service)
            });

            return serviceAtLocations;
        }

        private Location GetLocations(Record service)        {
            var location = new Location();
            location.Id = Guid.NewGuid().ToString();
            location.Name = service.Name.Substring(0, service.Name.Length > 50 ? 50 : service.Name.Length);
            location.Description = service.Name.Substring(0, service.Name.Length > 50 ? 50 : service.Name.Length);

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

        private ServiceType GetServiceType()
        {
            return new ServiceType() { Id = "2", Name = "FX", Description = "Family Experience" };
        }

        private List<CostOption> GetCostOptions(Record service)
        {
            var CostOptions = new List<CostOption>(); 
            foreach (var cost in service.CostOption ?? new List<CostTable>())
            {
                decimal costDecimal;
                
                CostOptions.Add(new CostOption()
                {
                    Id = Guid.NewGuid().ToString(),
                    Amount = decimal.TryParse(cost.CostAmount, out costDecimal) ? costDecimal : 0,
                    Amount_description = cost.CostType is null ? string.Empty : cost.CostType.Displayname.ToString()
                });
            }
            return CostOptions;
        }

        private List<PhysicalAddress> GetPhysicalAddress(Record service)
        {
            var PhysicalAddresses = new List<PhysicalAddress>();

            //CheckIfLocationExists(service.Postcode);

            PhysicalAddresses.Add(new PhysicalAddress()
            {
                Address_1 = string.IsNullOrEmpty(service.PublicAddress1) ? $"{service.PublicAddress2}, {service.PublicAddress3}" : $"{service.PublicAddress1}, {service.PublicAddress2},{service.PublicAddress3  }",
                City = service.PublicAddress4,
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

        public List<ContactNumbers> GetContactNumbers(List<string> contactPhoneNumbers)
        {
            List<ContactNumbers> contactNumbers = new();
            foreach (var contactNumber in contactPhoneNumbers ?? new List<string>())
            {
                contactNumbers.Add(new ContactNumbers() { Number = contactNumber, Id = Guid.NewGuid().ToString() });
            }

            return contactNumbers;
        }

        public static async Task<Rootobject> GetSalfordDataAsync()
        {
            _client = new();
           // _client.BaseAddress = new("https://api.openobjects.com/v2/salfordfsd/");
            //Rootobject servicesList = new();

            using var response = await _client.GetAsync(new Uri("https://api.openobjects.com/v2/salfordfsd/records?key=633eb0a9e4b0b3bc6d117a9a&startIndex=3&count=4&query=api_channel:familyhubs"));
            var apiResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Rootobject>(apiResponse);


            //using (StreamReader r = new StreamReader("D:\\DFE\\sampleJson1.json"))
            //{
            //    string json = r.ReadToEnd();
            //    serviceList = JsonConvert.DeserializeObject<Rootobject>(json);
            //}

            

        }

        private static Mapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMappingProfiles());

            });
            return new Mapper(config);

        }
    }
}

