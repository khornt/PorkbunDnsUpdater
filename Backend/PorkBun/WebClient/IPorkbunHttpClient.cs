using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PorkbunDnsUpdater.Backend.PorkBun.Dto.Response;
using PorkbunDnsUpdater.Backend.PorkBun.Dto.Response.JsonToCSharp;

namespace PorkbunDnsUpdater.Backend.PorkBun.WebClient
{
    public interface IPorkbunHttpClient
    {

        Task<PingV4Response> Ping(CancellationToken ct);      
        Task<PorkbunRecordResponse?> GetPorkbunRecord(string domain, string type, string subdomain, CancellationToken ct);
        Task<PorkbunRecordResponse> UpdatePorkbunRecord(string domain, string type, string subdomain, string myIp, CancellationToken ct);

        
    }
}
