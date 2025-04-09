﻿using System.Threading.Tasks;
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

        CancellationTokenSource cts;
        private readonly INavigationService _navigationService;
        private readonly PorkbunUpdaterService _porkbunUpdaterService;
        private readonly AppConfig _appConfig;

        private string _currentV4Ip;
        private string _currentV6Ip;
        private string _dnsHost;
        private string _dnsDomain;
               

        private string _checkInterval;
        private string _dnsRecord;
        private string _dnsProgress;
        private bool _showStartButton;
        private bool _showStopButton;


        public DnsUpdaterViewModel(INavigationService navigationService, AppConfig appConfig, PorkbunUpdaterService porkbunUpdaterService)
        {
            _navigationService = navigationService;
            _appConfig = appConfig;
            _porkbunUpdaterService = porkbunUpdaterService;

            IntervalDropDown = appConfig.PorkbunIntervals;
            StartDnsUpdater = new TaskDelegateCommand(ExecuteStartDnsUpdater, CanExecuteStartDnsUpdater).ObservesProperty(() => DnsProgress);
            StopDnsUpdater = new TaskDelegateCommand(ExecuteStopDnsUpdater);
            _showStopButton = false;
            _showStartButton = true;
        }


        public ICommand StartDnsUpdater { get; private set; }

        public ICommand StopDnsUpdater { get; private set; }
         
        //public bool ShowStartButton { get; set; } = true;
        //public bool ShowStopButton { get; set; } = true;    

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


        public bool ShowStopButton
        {
            get { return _showStopButton; }
            set { _showStopButton = value; OnPropertyChanged("ShowStopButton"); }
        }

        public bool ShowStartButton
        {
            get { return _showStartButton; }
            set { _showStartButton = value; OnPropertyChanged("ShowStartButton"); }
        }

        public List<string> IntervalDropDown { get; }


        private async Task ExecuteStartDnsUpdater()
        {
            ShowStartButton = false;
            ShowStopButton = true;
            var justNow = DateTimeOffset.Now;
            cts = new CancellationTokenSource();

            DnsProgress = "";

            var check = await QuickCheckConfig();

            if (!check) return;

            StatusWindowUpdater("Starting up DnsUpdater!!", false);
            
            if (string.IsNullOrEmpty(CheckInterval))
            {
                StatusWindowUpdater("Default check interval: 60  min");                
            }
            
            var checkInterval = IntervalConverter(CheckInterval);
            Progress<StatusReport> report = new Progress<StatusReport>();

            report.ProgressChanged += DnsUpdaterReport;

            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += DnsUpdaterRrogress;

            if (_currentV4Ip == null)
            {                
                var response = await _porkbunUpdaterService.InitPorkbunUpdater(_dnsDomain, "A", _dnsHost, report, cts.Token);

                if (response == "")
                {
                    StatusWindowUpdater("STOP!!");
                    ShowStartButton = true;
                    ShowStopButton = false;
                    return;
                }               
                _currentV4Ip = response;                
            }

            CurrentV4iP = _currentV4Ip;

            try
            {
                await _porkbunUpdaterService.ContinuouslyUpdate(_dnsDomain, "A", _dnsHost, checkInterval, CurrentV4iP, report, progress, cts.Token);

            }
            catch (OperationCanceledException)
            {
                StatusWindowUpdater("STOP!!");
            }                        
        }

        private async Task ExecuteStopDnsUpdater()
        {

            cts.Cancel();
            ShowStartButton = true;
            ShowStopButton = false;
        }

        private async Task<bool> QuickCheckConfig()
        {
            var key = _appConfig.PorkbunApiKey;
            var secret = _appConfig.PorkbunApiSecret;

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret)) 
            {

                StatusWindowUpdater("Missing ApiKey / Secret config...", false);                
                await Task.Delay(2000);
                StatusWindowUpdater("STOP!!");
                return false;
            }

            if (string.IsNullOrEmpty(_dnsDomain))
            {
                StatusWindowUpdater("Enter Domain to update...", false);                
                await Task.Delay(2000);
                StatusWindowUpdater("STOP!!");
                return false;
            }

            if (string.IsNullOrEmpty(_dnsHost))
            {
                StatusWindowUpdater("Enter hostname to update...", false);
                await Task.Delay(2000);
                StatusWindowUpdater("STOP!!");
                return false;
            }

            return true;
        }

        private void DnsUpdaterReport(object? sender, StatusReport e)
        {
            StatusWindowUpdater(e.Content, e.Newline);
        }
        
        private void DnsUpdaterRrogress(object? sender, ProgressReport e)   
        {
            var her = e.Content;
            CurrentV4iP = e.Message + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

        }


        private void StatusWindowUpdater(string logg, bool newLine = true)
        {

            var textToPrint = "";
            if (newLine) 
            { 
                textToPrint = Environment.NewLine;
                textToPrint += DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + ": ";
            }
            
            textToPrint += logg;
            DnsProgress += textToPrint;
        }

        private bool CanExecuteStartDnsUpdater()
        {
            return true;
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
