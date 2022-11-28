namespace DataMapping.Models.API
{
    public class ServiceAtLocation
    {
        public string Id { get; set; }
        public Location LocationDetails { get; set; }
        public List<RegularSchedule>? RegularSchedules { get; set; }
        public List<HolidaySchedule>? HolidaySchedules { get; set; }
    }
}
