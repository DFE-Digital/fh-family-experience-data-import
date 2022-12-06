using FamilyExperience.DataImport.Models;
using Newtonsoft.Json;

namespace FamilyExperience.DataImport.Service
{
    public class PostcodeLocationService
    {
        private static readonly HttpClient HttpClient = new HttpClient
        {
            BaseAddress = new Uri("http://api.postcodes.io")
        };

        private readonly List<LongitudeLatitude> _longitudeLatitudes = new List<LongitudeLatitude>();

        private async Task<PostCode> LookupPostcode(string postcode)
        {
            var response = await HttpClient.GetAsync($"/postcodes/{postcode}", HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode) return new PostCode();

            var responseString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<PostCode>(responseString);
        }

        public LongitudeLatitude GetLongitudeLatitudeForPostcode(string postcode)
        {
            if (_longitudeLatitudes is null || _longitudeLatitudes.Count <= 0)
                return PostCodeLookUp(postcode);
            
            var details = _longitudeLatitudes.Where(x => x.Postcode == postcode).ToList();
            
            return details.Count >= 1 ? details.FirstOrDefault() : PostCodeLookUp(postcode);
        }

        private LongitudeLatitude PostCodeLookUp(string postcode)
        {
            var result = LookupPostcode(postcode).Result;
            
            if (result.Result is null) return new LongitudeLatitude();

            var longitudeLatitude = new LongitudeLatitude
            {
                Postcode = result.Result.Postcode,
                Latitude = result.Result.Latitude,
                Longitude = result.Result.Longitude,
                AdministrativeDistrictCode = result.Result.Codes.AdminDistrict
            };

            _longitudeLatitudes.Add(longitudeLatitude);
            
            return longitudeLatitude;
        }
    }
}