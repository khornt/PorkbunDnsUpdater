using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PorkbunDnsUpdater.Backend.PorkBun.Dto.Response;

namespace PorkbunDnsUpdater.Backend.PorkBun.WebClient
{
    public interface IPorkbunHttpClient
    {

        Task<PingV4Response> Ping();
        //Task<bool> PostUpdateChallengeRecord(string challenge, string fqdn);

        Task<string?> GetPorkbunRecord(string domain, string type, string subdomain);

        Task<string?> UpdatePorkbunRecord(string domain, string type, string subdomain, string myIp);

        
    }
}
