using System.Windows.Input;
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

        private string _pingV4Response;
        private string _pingV6Response;

        private string _aRecordIp;

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

            DnsProgress = "Starting up DnsUpdater!!";
            DnsRecord = "fsdfjdsffe";


            

            var checkInterval = IntervalConverter(CheckInterval);
            

            if (_currentV4Ip == null)
            {
                var result = await _porkbunUpdaterService.PingPorkbun();


                if (result.Status != "Error")
                {
                    DnsProgress = DnsProgress + "\n" + "Your public IP is: " + result.YourIp;
                    var myRealIp = result.YourIp;
                    if (myRealIp != _currentV4Ip)
                    {
                        var currentDnsIp = await _porkbunUpdaterService.GetPorkbunRecord("lekestue.me", "A", "fw");

                        DnsProgress = DnsProgress + "\n" + "Your DNS Ip is: " + currentDnsIp.ToString();

                        if (!string.IsNullOrEmpty(currentDnsIp))
                        {
                            if (myRealIp != currentDnsIp)
                            {

                                DnsProgress = DnsProgress + "\n" + "Updating DNS Record...";
                                await _porkbunUpdaterService.UpdateDnsRecord("lekestue.me", "A", "fw", myRealIp);

                                DnsProgress = DnsProgress + "\n" + "Done!";
                            }
                            else if (myRealIp == currentDnsIp)
                            {
                                CurrentV4iP = currentDnsIp;
                            }
                        }
                        else
                        {
                            DnsProgress = "Error";
                            //Host og record ekisterer ikke, lag det og restart
                            return;
                        }
                    }
                }
                else
                {
                    DnsProgress = "Error";
                    return; //init error, dispaly something
                }
            }


            Progress<string> progress = new Progress<string>();
            progress.ProgressChanged += DnsUpdaterReport;


            //alles gut 
            await _porkbunUpdaterService.ContinuouslyUpdate(checkInterval, CurrentV4iP, progress);
            //infinit loop here!!
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
