using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyExperience.Dataimport.Models.API
{
    public class Service
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }

        public List<Eligibility> Eligibilities { get; set; }
        public List<Location> Locations { get; set; }


    }
}
