using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PorkbunDnsUpdater.Backend.PorkBun.Dto.Response
{
    public class PingV4Response
    {

        [JsonProperty("status")]
        public string Status {  get; set; }
        
        [JsonProperty("yourIp")]
        public string YourIp { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        //[JsonProperty("yourIp")]
        //public string YourIp { get; set; }
    }
}
