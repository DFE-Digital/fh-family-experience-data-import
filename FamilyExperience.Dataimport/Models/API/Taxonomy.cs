using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyExperience.Dataimport.Models.API
{
    public class Taxonomy
    {
         public string Id { get; set; }
        public string Name { get; set; }
        public string Vocabulary { get; set; }
        public string Parent { get; set; }

    }
}
