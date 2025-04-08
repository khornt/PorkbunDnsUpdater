using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;
using PorkbunDnsUpdater.Backend.PorkBun.WebClient;
using PorkbunDnsUpdater.Commands;
using PorkbunDnsUpdater.Models;
using PorkbunDnsUpdater.Services;

namespace PorkbunDnsUpdater.ViewModels
{
    public class DnsUpdaterViewModel : ViewModelBase
    {

        private readonly INavigationService _navigationService;
        private readonly PorkbunUpdaterService _porkbunUpdaterService;
        private readonly AppConfig _appConfig;

        private string _currentV4Ip;
        private string _currentV6Ip;

        private string _dnsHost;
        private string _dnsDomain;

        //private string _pingV4Response;
        //private string _pingV6Response;

        //private string _aRecordIp;

        private string _checkInterval;
        private string _dnsRecord;
        private string _dnsProgress;


        public DnsUpdaterViewModel(INavigationService navigationService, AppConfig appConfig, PorkbunUpdaterService porkbunUpdaterService)
        {
            _navigationService = navigationService;
            _appConfig = appConfig;
            _porkbunUpdaterService = porkbunUpdaterService;

            IntervalDropDown = appConfig.PorkbunIntervals;
            StartDnsUpdater = new TaskDelegateCommand(ExecuteStartDnsUpdater, CanExecuteStartDnsUpdater).ObservesProperty(() => DnsProgress);
            StopDnsUpdater = new TaskDelegateCommand(ExecuteStopDnsUpdater);
        }


        public ICommand StartDnsUpdater { get; private set; }

        public ICommand StopDnsUpdater { get; private set; }

        public string CurrentV4iP
        {
            get { return _currentV4Ip; }
            set { _currentV4Ip = value; OnPropertyChanged("CurrentV4iP"); }
        }

        public string CurrentV6iP
        {
            get { return _currentV6Ip; }
            set { _currentV6Ip = value; OnPropertyChanged("CurrentV6iP"); }
        }

        public string DnsHost
        {
            get { return _dnsHost; }
            set { _dnsHost= value; OnPropertyChanged("DnsHost"); }
        }

        public string DnsDomain
        {
            get { return _dnsDomain; }
            set { _dnsDomain = value; OnPropertyChanged("DnsDomain"); }
        }

        public string DnsRecord
        {
            get { return _dnsRecord; }
            set { _dnsRecord = value; OnPropertyChanged("DnsRecord"); }
        }

        public string CheckInterval
        {
            get { return _checkInterval; }
            set { _checkInterval = value; OnPropertyChanged("CheckInterval"); }
        }


        public string DnsProgress
        {
            get { return _dnsProgress; }
            set { _dnsProgress = value; OnPropertyChanged("DnsProgress"); }
        }

        public List<string> IntervalDropDown { get; }


        private async Task ExecuteStartDnsUpdater()
        {
            var justNow = DateTimeOffset.Now;

            DnsProgress = "";

            var check = await QuickCheckConfig();

            if (!check) return;

            DnsProgress = "Starting up DnsUpdater!!";


            if (string.IsNullOrEmpty(CheckInterval))
            {
                DnsProgress += "\nDefault check interval: 60  min";
            }
            
            var checkInterval = IntervalConverter(CheckInterval);
            Progress<string> progress = new Progress<string>();
            progress.ProgressChanged += DnsUpdaterReport;

            if (_currentV4Ip == null)
            {                
                CurrentV4iP = await _porkbunUpdaterService.InitPorkbunUpdater(_dnsDomain, "A", _dnsHost, progress);
                //Sjekk status om den er initialisert ok
            }
            
            //alles gut 
            await _porkbunUpdaterService.ContinuouslyUpdate(_dnsDomain, "A", _dnsHost, checkInterval, CurrentV4iP ,progress);
            //infinit loop here!!
        }

        private async Task<bool> QuickCheckConfig()
        {
            var key = _appConfig.PorkbunApiKey;
            var secret = _appConfig.PorkbunApiSecret;

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret)) 
            {
                DnsProgress = "Missing ApiKey / Secret config...";
                await Task.Delay(2000);
                DnsProgress += "Stop!";
                return false;
            }

            if (string.IsNullOrEmpty(_dnsDomain))
            {
                DnsProgress = "Enter Domain to update...";
                await Task.Delay(2000);
                DnsProgress += "Stop!";
                return false;
            }

            if (string.IsNullOrEmpty(_dnsHost))
            {
                DnsProgress = "Enter host name to update...";
                await Task.Delay(2000);
                DnsProgress += "Stop!";
                return false;
            }

            return true;
        }

        private void DnsUpdaterReport(object? sender, string e)
        {
            DnsProgress += e;
        }

        private bool CanExecuteStartDnsUpdater()
        {
            return true;
        }

        private async Task ExecuteStopDnsUpdater()
        {
            return;
        }

        private int IntervalConverter(string name)
        {
            switch (name)
            {
                case "15 min":
                    return 15;
                case "30 min":
                    return 30;
                case "1 hour":
                    return 60;
                case "3 hour":
                    return 180;
                case "6 hour":
                    return 360;
                case "12 hour":
                    return 720;
                case "1 day":
                    return 1440;
                case "1 min":
                    return 1;
                default:
                    return 60;
            }
        }
    }
}
