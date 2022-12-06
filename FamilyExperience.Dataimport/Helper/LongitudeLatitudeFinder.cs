using FamilyExperience.Dataimport.Models.API;
using FamilyExperience.Dataimport.Service;

namespace FamilyExperience.Dataimport.Helper
{
    public class LongitudeLatitudeFinder
    {
        private List<LongitudeLatitude> longitudeLatitudes = new List<LongitudeLatitude>();
        public LongitudeLatitude? GetLongitudeLatitudeForPostcode(string postcode, PostcodeLocationService _postcodeLocationService)
        {
            var longitudeLatitude = new LongitudeLatitude();
            if (longitudeLatitudes is not null && longitudeLatitudes.Count > 0)
            {
                var details = longitudeLatitudes.Where(x => x.Postcode == postcode).ToList();
                return details.Count >= 1 ? details.FirstOrDefault() : PostCodeLookUp(postcode, longitudeLatitude, _postcodeLocationService);
            }

            return PostCodeLookUp(postcode, longitudeLatitude, _postcodeLocationService);
        }

        private LongitudeLatitude PostCodeLookUp(string postcode, LongitudeLatitude longitudeLatitude, PostcodeLocationService _postcodeLocationService)
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
    }
}
