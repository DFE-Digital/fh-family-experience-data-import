using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;
using Newtonsoft.Json;

namespace FamilyExperience.Dataimport.Models
{
  

        public class RootTaxonomyobject
        {
            public OpenReferralTaxonomyDto[] items { get; set; }
            public int pageNumber { get; set; }
            public int totalPages { get; set; }
            public int totalCount { get; set; }
            public bool hasPreviousPage { get; set; }
            public bool hasNextPage { get; set; }
        }

        
    
}
