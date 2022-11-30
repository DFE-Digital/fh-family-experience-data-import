namespace FamilyExperience.Dataimport.Models.API
{
    public class HolidaySchedule
    {
        public string Id { get; set; }
        public bool Closed { get; set; }
        public DateTime? OpensAt { get; set; }
        public DateTime? ClosesAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }        
    }
}
