using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralRegularSchedule;

namespace FamilyExperience.Dataimport.Models.API
{
    public class ServiceAtLocation
    {
        public string Id { get; set; }
        public Location LocationDetails { get; set; }
        public List<OpenReferralRegularScheduleDto>? RegularSchedules { get; set; }
        public List<HolidaySchedule>? HolidaySchedules { get; set; }
    }
}
