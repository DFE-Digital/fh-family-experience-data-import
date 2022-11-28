namespace DataMapping.Models.API
{
    public class Location
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }        
        public List<PhysicalAddress>? PhysicalAddresses { get; set; }
    }
}
