using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyExperienced.Dataimport.Models.Json
{
    public class THService
{
        [JsonProperty("reg_name")]
        public string LocationName { get; set; }

        [JsonProperty("provider_name")]
        public string ServiceName { get; set; }

        [JsonProperty("provider_start")]
        public string ValidFrom { get; set; }

        [JsonProperty("provider_end")]
        public string ValidTo { get; set; }

        [JsonProperty("provider_telephone")]
        public string Number { get; set; }

        [JsonProperty("provider_web_site")]
        public string Url { get; set; }

        [JsonProperty("provider_email")]
        public string Email { get; set; }



    }
}
