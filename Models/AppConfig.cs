using System.Configuration;

namespace PorkbunDnsUpdater.Models
{
    public class AppConfig
    {
        public string ContactEmail => GetValue(nameof(ContactEmail));
                
        public string CertificatePassword => GetValue(nameof(CertificatePassword));

        public string PorkbunApiUrl => GetValue(nameof(PorkbunApiUrl));
                

        public string PorkbunApiKey => GetValue(nameof(PorkbunApiKey));

        public string PorkbunApiSecret => GetValue(nameof(PorkbunApiSecret));


        public List<string> PorkbunIntervals
        {
            get
            {
                var listAsString = GetValue(nameof(PorkbunIntervals));
                return listAsString.Split(",").ToList();
            }
        }

        public List<string> RsaKeys
        {
            get
            {
                var listAsString = GetValue(nameof(RsaKeys));
                return listAsString.Split(",").ToList();
            }
        }

        private string GetValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}

