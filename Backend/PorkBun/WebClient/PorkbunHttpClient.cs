
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PorkbunDnsUpdater.Backend.PorkBun.Dto.Request;
using PorkbunDnsUpdater.Backend.PorkBun.Dto.Response;
using PorkbunDnsUpdater.Backend.PorkBun.Dto.Response.JsonToCSharp;
using PorkbunDnsUpdater.Models;

namespace PorkbunDnsUpdater.Backend.PorkBun.WebClient
{
    public class PorkbunHttpClient : IPorkbunHttpClient
    {
                
        private readonly AppConfig _appConfig;
        private static HttpClient _httpClient;  //di
        private static JsonSerializerSettings _jsonFomatter; //di


        public PorkbunHttpClient(AppConfig appConfig)
        {
            _appConfig = appConfig;
            _httpClient = new HttpClient();
            _jsonFomatter = JsonFormatter();
        }


        public async Task<PingV4Response?> Ping(CancellationToken ct)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, _appConfig.PorkbunApiUrl + "/ping"))
                {                   
                    request.Content = new StringContent(CreatePingRequest(), Encoding.UTF8, "application/json");

                    using (var httpResponse = await _httpClient.SendAsync(request, ct))
                    {
                        if (httpResponse.IsSuccessStatusCode)
                        {
                            var response = await httpResponse.Content.ReadAsStringAsync();
                            var dtoObject = JsonConvert.DeserializeObject<PingV4Response>(response);

                            return dtoObject;
                        }
                        else
                        {
                            var response = await httpResponse.Content.ReadAsStringAsync();
                            var dtoObject = JsonConvert.DeserializeObject<PingV4Response>(response);
                            return dtoObject;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<PorkbunRecordResponse?> GetPorkbunRecord(string domain, string type, string subdomain, CancellationToken ct)

        {                        
            var requestUrl = _appConfig.PorkbunApiUrl + "/dns/retrieveByNameType/" + domain + "/" + type + "/" + subdomain;

            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUrl))
            {
                var stringPayload = CreateRequest();
                request.Content = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                using (var httpResponse = await _httpClient.SendAsync(request, ct))
                {
                    var response = await httpResponse.Content.ReadAsStringAsync();
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        
                        var respDto = JsonConvert.DeserializeObject<PorkbunRecordResponse>(response);                                                 
                        return respDto;
                    }
                    else
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<PorkbunRecordResponse>(response);

                        } catch
                        {
                            return new PorkbunRecordResponse
                            {
                                Status = "ERROR",
                                Message = "Failed to deserialize message from Porkbun"
                            };
                        }                                                
                    }
                }
            }         
        }


        public async Task<PorkbunRecordResponse?> UpdatePorkbunRecord(string domain, string type, string subdomain, string myNewIp, CancellationToken ct)
        {
            try
            {                
                var requestUrl = _appConfig.PorkbunApiUrl + "/dns/editByNameType/" + domain + "/" + type + "/" + subdomain;

                using (var request = new HttpRequestMessage(HttpMethod.Post, requestUrl))
                {
                    var stringPayload = CreateRequest(myNewIp);
                    request.Content = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                    using (var httpResponse = await _httpClient.SendAsync(request, ct))
                    {
                        var response = await httpResponse.Content.ReadAsStringAsync();

                        if (httpResponse.IsSuccessStatusCode)
                        {                            
                            var responseDto = JsonConvert.DeserializeObject<PorkbunRecordResponse>(response);
                            return responseDto; 
                        }
                        else
                        {
                            var responseDto = JsonConvert.DeserializeObject<PorkbunRecordResponse>(response);
                            return responseDto;
                        }
                    }
                }
            }
            catch
            {
                return new PorkbunRecordResponse
                {
                    Status = "ERROR",
                    Message = "Failed to deserialize message from Porkbun"
                };
            }
        }

        private string CreateRequest()
        {

            var dto = new PorkbunDto
            {
                Apikey = _appConfig.PorkbunApiKey,
                Secretapikey = _appConfig.PorkbunApiSecret
            };

            var jsonPayload = JsonConvert.SerializeObject(dto, _jsonFomatter);
            return jsonPayload;
                      
        }

        private string CreateRequest(string myNewIp)
        {

            var dto = new PorkbunDto
            {
                Apikey = _appConfig.PorkbunApiKey,
                Secretapikey = _appConfig.PorkbunApiSecret,
                Content = myNewIp,
                Ttl = "600"

            };

            var jsonPayload = JsonConvert.SerializeObject(dto, _jsonFomatter);
            return jsonPayload;

        }




        private string CreatePingRequest()
        {
            var dto = new PorkbunDto
            {
                Apikey = _appConfig.PorkbunApiKey,
                Secretapikey = _appConfig.PorkbunApiSecret
            };

            return JsonConvert.SerializeObject(dto, _jsonFomatter);             
        }

        private JsonSerializerSettings JsonFormatter()
        {
            var formatter = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ"
            };

            return formatter;
        }
                
    }
}
