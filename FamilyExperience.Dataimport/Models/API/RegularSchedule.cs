namespace FamilyExperience.Dataimport.Models.API
{
    public class RegularSchedule
    {
        public string Id { get; set; }   
        public string ServiceId { get; set; }
        public string Description { get; set; }
        public DateTime? OpensAt { get; set; }
        public DateTime? ClosesAt { get; set; }
        public string? Byday { get; set; }
        public string? Bymonthday { get; set; }
        public string? Dtstart { get; set; }
        public string? Freq { get; set; }
        public string? Interval { get; set; }
        public DateTime?  ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public int Weekday { get; set; }


    }
}
