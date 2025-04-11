using System.Text;
using PorkbunDnsUpdater.Backend.PorkBun.Dto.Response.JsonToCSharp;
using PorkbunDnsUpdater.Backend.PorkBun.WebClient;
using PorkbunDnsUpdater.Models;

namespace PorkbunDnsUpdater.Services
{
    public class PorkbunUpdaterService
    {

        private readonly IPorkbunHttpClient _httpClient;

        public PorkbunUpdaterService(IPorkbunHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        
        public async Task<string> InitDnsUpdaterdater(Record record, IProgress<StatusReport> report, CancellationToken ct) //progress skal inneholde en del mere
        {
            var realIp = await _httpClient.Ping(ct);
                      

            if (realIp?.YourIp != null && realIp?.Status != "Error")
            {
                var reportBack = "Your public IP is: " + realIp.YourIp;
                report.Report(new StatusReport { Content = reportBack });

                var responseDto = await _httpClient.GetPorkbunRecord(record, ct);

                if (responseDto == null)
                {
                    reportBack = "Initialization faild with empty response from Porkbun";
                    report.Report(new StatusReport { Content = reportBack });                    
                    return "";
                }
                else if (responseDto?.Status == "ERROR")
                {

                    reportBack = $"Error getting record from Porkbun: {responseDto.Message} ";
                    report.Report(new StatusReport { Content = reportBack });                    
                    return "";
                }
                else if (responseDto?.Status == null)
                {

                    reportBack = "WTF??";
                    report.Report(new StatusReport { Content = reportBack });                    
                    return "";
                }
                var currentDnsIp = responseDto?.Records?.First().Content;

                if (!string.IsNullOrEmpty(currentDnsIp))
                {

                    reportBack = "Your DNS IP is: " + currentDnsIp.ToString();
                    report.Report(new StatusReport { Content = reportBack });
                                        
                    if (realIp.YourIp != currentDnsIp)
                    {

                        reportBack = "Updating DNS PorkbunRecord...";
                        report.Report(new StatusReport { Content = reportBack });
                                                                        
                        var response = await _httpClient.UpdatePorkbunRecord(record, realIp.YourIp, ct);

                        if (response != null && response.Status == "SUCCESS")
                        {

                            report.Report(new StatusReport { Content = "Done!", Newline = false });                            
                            return realIp.YourIp;
                        }
                    }
                    else 
                    {
                        return realIp.YourIp;
                    }                   
                }                
            }

            report.Report(new StatusReport { Content = "Error!!"});            
            return "";
        }


        public async Task ContinuouslyUpdate(Record record, int interval, string currentIp,IProgress<StatusReport> report, IProgress<ProgressReport> progress, CancellationToken ct) //progress skal inneholde en del mere
        {
            var intervalInSecounds = interval * 60;
            var runIt = true;
            //var pingPong = "|                    |";
                        
            var nextUpdateDue = DateTimeOffset.UtcNow.AddSeconds(intervalInSecounds);

            while (runIt)
            {
                var justNow = DateTimeOffset.UtcNow;
                if (nextUpdateDue < justNow)
                {
                    report.Report(new StatusReport { Content = "Just checking..." });
                    nextUpdateDue = DateTimeOffset.UtcNow.AddSeconds(interval);
                     var result = await _httpClient.Ping(ct);

                    var callBack = new ProgressReport
                    {
                        Ip4 = result?.YourIp ?? "",
                        Message = result?.Message ?? "",
                        Content = result?.Status ?? ""

                    };

                    progress.Report(callBack);                                       
                    if (result?.YourIp != null && result.YourIp != currentIp)
                    {
                        report.Report(new StatusReport { Content = "IP has changed!!" });
                        
                        var updateResponse = await _httpClient.UpdatePorkbunRecord(record, result.YourIp, ct);

                        if (updateResponse?.Status == "SUCCESS")
                        {
                            var reportBack = $"DNS PorkbunRecord has been updated with IP: {result.YourIp}";
                            report.Report(new StatusReport { Content = reportBack });
                            currentIp = result.YourIp;

                        } else
                        {
                            var message = updateResponse?.Message ?? "";
                            var reportBack = $"Eror updating DNS record: {message}";
                            report.Report(new StatusReport { Content = reportBack });
                        }                        
                    }
                    if (result?.YourIp == null)
                    {
                        var reportBack = $"Missing IP from Ping request";
                        report.Report(new StatusReport { Content = reportBack });

                    }
                    else 
                    {
                        var reportBack = new StatusReport 
                            { Content = "Done!", 
                               Newline = false,
                        };
                        report.Report(reportBack);
                    }
                        nextUpdateDue = DateTimeOffset.UtcNow.AddSeconds(intervalInSecounds);
                }                
                ct.ThrowIfCancellationRequested();
                await Task.Delay(1000, ct);
            }            
        }        
    }
}
