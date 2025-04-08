
using System.DirectoryServices.ActiveDirectory;
using System.Net;
using Org.BouncyCastle.Bcpg.OpenPgp;
using PorkbunDnsUpdater.Backend.PorkBun.Dto.Response;
using PorkbunDnsUpdater.Backend.PorkBun.Dto.Response.JsonToCSharp;

namespace PorkbunDnsUpdater.Backend.PorkBun.WebClient
{
    public class PorkbunUpdaterService
    {

        private readonly IPorkbunHttpClient _httpClient;

        public PorkbunUpdaterService(IPorkbunHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> UpdateDnsRecord(string domain, string type, string subdomain, string myIp)
        {            
            return await _httpClient.UpdatePorkbunRecord(domain, type, subdomain, myIp);            

        }

        //public async Task<PingV4Response> PingPorkbun()
        //{
        //    return await _httpClient.Ping();
        //}

        public async Task<string?> GetPorkbunRecord(string domain, string type, string subdomain)
        {

            return await _httpClient.GetPorkbunRecord(domain, type, subdomain);            
        }

        public async Task<string> InitPorkbunUpdater(string domain, string type, string subdomain, IProgress<string> progress) //progress skal inneholde en del mere
        {
            var realIp = await _httpClient.Ping();
            

            if (realIp.Status != "Error")
            {

                progress.Report("\n" + "Your public IP is: " + realIp.YourIp);
                              

                var currentDnsIp = await _httpClient.GetPorkbunRecord(domain, type, subdomain);

                //Mer sjekk her

                
                progress.Report("\nYour DNS Ip is: " + currentDnsIp.ToString());


                if (!string.IsNullOrEmpty(currentDnsIp))
                {
                    if (realIp.YourIp != currentDnsIp)
                    {

                        progress.Report("\nUpdating DNS Record...");
                        var newRealIp = await _httpClient.UpdatePorkbunRecord(domain, type, subdomain, realIp.YourIp);
                        progress.Report("Done!");
                        return newRealIp;

                    } else
                    {
                        return realIp.YourIp;
                    }
                    
                }
                else
                {
                    progress.Report("\nError!!");                    
                    return "";
                }
                
            }
            else
            {
                progress.Report("\nError!!");
                return "";
            }         
        }


        public async Task<bool> ContinuouslyUpdate(string domain, string type, string subdomain, int interval,string currentIp,IProgress<string> progress) //progress skal inneholde en del mere
        {
            var intervalInSecounds = interval * 60;
            var runIt = true;
            var fake = 0;
            
            var nextUpdateDue = DateTimeOffset.UtcNow.AddSeconds(intervalInSecounds);

            while (runIt)
            {
                var justNow = DateTimeOffset.UtcNow;
                if (nextUpdateDue < justNow)
                {
                    progress.Report("\nJust checking...");
                    nextUpdateDue = DateTimeOffset.UtcNow.AddSeconds(interval);
                     var result = await _httpClient.Ping();
                    
                    fake++;
                    if (fake == 30)
                    {
                        progress.Report("\nDebug fake, setting ip to some shit...");
                        result.YourIp = "192.168.50.2"; //berre skit!!
                        fake = 0;
                    }

                    if (result.YourIp != currentIp)
                    {
                        progress.Report("Ip has changed!!");
                        var updateResponse = await _httpClient.UpdatePorkbunRecord("lekesute.me", "A", "fw", result.YourIp);

                        if (updateResponse != null)
                        {
                            progress.Report($"\nDNS Record has been updated with IP: {updateResponse} ");  //Få denne over i dto og sjekk status først.
                        }                                                

                    }
                    else 
                    {
                        progress.Report("Done!");
                    }
                        nextUpdateDue = DateTimeOffset.UtcNow.AddSeconds(intervalInSecounds);
                }
                await Task.Delay(5000);
            }

            return true;


        }
        //public async Task<string?> UpdatePorkbunRecord(string domain, string type, string subdomain)
        //{

        //    return await _httpClient.UpdatePorkbunRecord(domain, type, subdomain);
        //}

    }
}
