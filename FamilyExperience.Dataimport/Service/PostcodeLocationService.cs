using FamilyExperience.Dataimport.Models.Json;
using System.Text.Json;

namespace FamilyExperience.Dataimport.Service
{
    public class PostcodeLocationService
    {
        readonly HttpClient _httpClient;
        public PostcodeLocationService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://api.postcodes.io")
            };
        }

        public async Task<PostCode> LookupPostcode(string postcode)
        {
            using var response = await _httpClient.GetAsync($"/postcodes/{postcode}", HttpCompletionOption.ResponseHeadersRead);

            return response.IsSuccessStatusCode ? 
             await JsonSerializer.DeserializeAsync<PostCode>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) :  new PostCode();


        }
    }     
}
