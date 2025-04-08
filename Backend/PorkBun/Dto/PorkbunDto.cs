using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PorkbunDnsUpdater.Backend.PorkBun.Dto
{
    public class PorkbunDto
    {
        [JsonProperty("apikey")]
        public string Apikey { get; set; }

        [JsonProperty("secretapikey")]
        public string Secretapikey {  get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("ttl")]
        public string Ttl { get; set; }

    }
}
