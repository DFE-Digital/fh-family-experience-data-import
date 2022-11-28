namespace DataMapping.Models.Json
{
    public class PostCode
    {
        public int status { get; set; }
        public Result result { get; set; }
    }

    public class Result
    {
        public string postcode { get; set; } = default!;
        public int quality { get; set; }
        public int eastings { get; set; }
        public int northings { get; set; }
        public string country { get; set; } = default!;
        public string nhs_ha { get; set; } = default!;
        public float longitude { get; set; }
        public float latitude { get; set; }
        public string region { get; set; } = default!;
        public Codes codes { get; set; } = default!;
    }



    public class Codes
    {
        public string admin_district { get; set; }
        public string admin_county { get; set; } 
        public string admin_ward { get; set; } 
        public string parish { get; set; } 
        public string parliamentary_constituency { get; set; } 
        
    }
}
