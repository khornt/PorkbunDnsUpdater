
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PorkbunDnsUpdater.Backend.PorkBun.Dto;
using PorkbunDnsUpdater.Backend.PorkBun.Dto.Response;
using PorkbunDnsUpdater.Backend.PorkBun.Dto.Response.JsonToCSharp;
using PorkbunDnsUpdater.Models;

namespace PorkbunDnsUpdater.Backend.PorkBun.WebClient
{
    public class PorkbunHttpClient : IPorkbunHttpClient
    {
                
        private readonly AppConfig _appConfig;
        private static HttpClient _httpClient;
        private static JsonSerializerSettings _jsonFomatter;


        public PorkbunHttpClient(AppConfig appConfig)
        {
            _appConfig = appConfig;
            _httpClient = new HttpClient();
            _jsonFomatter = JsonFormatter();
        }


        public async Task<PingV4Response> Ping()
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, _appConfig.PorkbunApiUrl + "/ping"))
                {                   
                    request.Content = new StringContent(CreatePingRequest(), Encoding.UTF8, "application/json");

                    using (var httpResponse = await _httpClient.SendAsync(request, CancellationToken.None))
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
                

        public async Task<bool> PostUpdateChallengeRecord(string challenge, string fqdn)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, _appConfig.PorkbunApiUrl + "/ping"))
                {
                    var stringPayload = CreateRequest();
                    request.Content = new StringContent(stringPayload, Encoding.UTF8, "application/json");


                    using (var httpResponse = await _httpClient.SendAsync(request, CancellationToken.None))
                    {
                        if (httpResponse.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }


        public async Task<string?> GetPorkbunRecord(string domain, string type, string subdomain)

        {
            try
            {
                string? curentPorkbunIp = null;
                var requestUrl = _appConfig.PorkbunApiUrl + "/dns/retrieveByNameType/" + domain + "/" + type + "/" + subdomain;


                using (var request = new HttpRequestMessage(HttpMethod.Post, requestUrl))
                {
                    var stringPayload = CreateRequest();
                    request.Content = new StringContent(stringPayload, Encoding.UTF8, "application/json");


                    using (var httpResponse = await _httpClient.SendAsync(request, CancellationToken.None))
                    {
                        if (httpResponse.IsSuccessStatusCode)
                        {
                            var response = await httpResponse.Content.ReadAsStringAsync();

                            var respDto = JsonConvert.DeserializeObject<PorkbunRecordResponse>(response);                         
                            if (respDto != null && respDto.Records.Count > 0)
                            {
                               curentPorkbunIp = respDto.Records.First().Content;
                            }

                            return curentPorkbunIp;
                        }
                        else
                        {
                            var response = await httpResponse.Content.ReadAsStringAsync();                            
                            return null;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }


        public async Task<string?> UpdatePorkbunRecord(string domain, string type, string subdomain, string myNewIp)
        {
            try
            {
                string? curentPorkbunIp = null;
                var requestUrl = _appConfig.PorkbunApiUrl + "/dns/editByNameType/" + domain + "/" + type + "/" + subdomain;


                using (var request = new HttpRequestMessage(HttpMethod.Post, requestUrl))
                {
                    var stringPayload = CreateRequest(myNewIp);
                    request.Content = new StringContent(stringPayload, Encoding.UTF8, "application/json");


                    using (var httpResponse = await _httpClient.SendAsync(request, CancellationToken.None))
                    {
                        if (httpResponse.IsSuccessStatusCode)
                        {
                            var response = await httpResponse.Content.ReadAsStringAsync();

                            var respDto = JsonConvert.DeserializeObject<PorkbunRecordResponse>(response);
                            if (respDto?.Records != null && respDto.Records.Count > 0)
                            {
                                curentPorkbunIp = respDto.Records.First().Content;
                            }

                            return curentPorkbunIp;
                        }
                        else
                        {
                            var response = await httpResponse.Content.ReadAsStringAsync();
                            return null;
                        }
                    }
                }
            }
            catch
            {
                return null;
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
