using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PorkbunDnsUpdater.Models
{
    public class ProgressReport
    {        
        public string? Content { get; set; }
        public string? Message { get; set; }
        public string? Ip4 { get; set; }
        public string? Ip6 { get; set; }
        public string? Bar { get; set; }
    }
}
