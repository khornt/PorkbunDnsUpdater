using PorkbunDnsUpdater.Backend.PorkBun.Dto.Response;
using PorkbunDnsUpdater.Backend.PorkBun.Dto.Response.JsonToCSharp;
using PorkbunDnsUpdater.Models;

namespace PorkbunDnsUpdater.Backend.PorkBun.WebClient
{
    public interface IPorkbunHttpClient
    {
        Task<PingV4Response?> Ping(CancellationToken ct);      
        Task<PorkbunRecordResponse?> GetPorkbunRecord(Record record, CancellationToken ct);
        Task<PorkbunRecordResponse?> UpdatePorkbunRecord(Record record, string myIp, CancellationToken ct);

        
    }
}
