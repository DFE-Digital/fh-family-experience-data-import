using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralTaxonomys;

namespace FamilyExperience.Dataimport.Models
{
    public class RootTaxonomyObject
    {
        public OpenReferralTaxonomyDto[] Items { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
