using System;
using System.Windows.Input;
using PorkbunDnsUpdater.Commands;
using PorkbunDnsUpdater.Models;
using PorkbunDnsUpdater.Services;

namespace PorkbunDnsUpdater.ViewModels
{
    public class DnsUpdaterViewModel : ViewModelBase
    {

        CancellationTokenSource? cts;
        private readonly INavigationService _navigationService;
        private readonly PorkbunUpdaterService _porkbunUpdaterService;
        private readonly AppConfig _appConfig;

        private string? _currentV4Ip;
        private string? _currentV6Ip;
        private string? _dnsHost;
        private string? _dnsDomain;

        private string _checkInterval;        
        private string? _dnsProgress;
        private bool _showStartButton;
        private bool _showStopButton;


        public DnsUpdaterViewModel(INavigationService navigationService, AppConfig appConfig, PorkbunUpdaterService porkbunUpdaterService)
        {
            _navigationService = navigationService;
            _appConfig = appConfig;
            _porkbunUpdaterService = porkbunUpdaterService;

            IntervalDropDown = appConfig.PorkbunIntervals;
            StartDnsUpdater = new TaskDelegateCommand(ExecuteStartDnsUpdater, CanExecuteStartDnsUpdater).ObservesProperty(() => DnsProgress);
            StopDnsUpdater = new DelegateCommand(ExecuteStopDnsUpdater);
            _showStopButton = false;
            _showStartButton = true;
        }


        public ICommand StartDnsUpdater { get; private set; }

        public ICommand StopDnsUpdater { get; private set; }
         

        public string? CurrentV4iP
        {
            get { return _currentV4Ip; }
            set { _currentV4Ip = value; OnPropertyChanged("CurrentV4iP"); }
        }

        public string? CurrentV6iP
        {
            get { return _currentV6Ip; }
            set { _currentV6Ip = value; OnPropertyChanged("CurrentV6iP"); }
        }

        public string? DnsHost
        {
            get { return _dnsHost; }
            set { _dnsHost = value; OnPropertyChanged("DnsHost"); }
        }

        public string? DnsDomain
        {
            get { return _dnsDomain; }
            set { _dnsDomain = value; OnPropertyChanged("DnsDomain"); }
        }
        
        public string CheckInterval
        {
            get { return _checkInterval; }
            set { _checkInterval = value; OnPropertyChanged("CheckInterval"); }
        }

        public string? DnsProgress
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

            var record = await GetDnsRecordToUpdate();
            
            if (record == null) return;

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
                var response = await _porkbunUpdaterService.InitDnsUpdaterdater(record, report, cts.Token);

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
                await _porkbunUpdaterService.ContinuouslyUpdate(record, checkInterval, CurrentV4iP, report, progress, cts.Token);
            }
            catch (OperationCanceledException)
            {
                StatusWindowUpdater("STOP!!");
            }                        
        }

        private void ExecuteStopDnsUpdater()
        {

            cts.Cancel();
            ShowStartButton = true;
            ShowStopButton = false;
        }

        private async Task<Record?> GetDnsRecordToUpdate()
        {
            var key = _appConfig.PorkbunApiKey;
            var secret = _appConfig.PorkbunApiSecret;

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret)) 
            {

                StatusWindowUpdater("Missing ApiKey / Secret config...", false);                
                await Task.Delay(2000);
                StatusWindowUpdater("STOP!!");
                return null;
            }

            if (string.IsNullOrEmpty(_dnsDomain))
            {
                StatusWindowUpdater("Enter Domain to update...", false);                
                await Task.Delay(2000);
                StatusWindowUpdater("STOP!!");
                return null;
            }

            if (string.IsNullOrEmpty(_dnsHost))
            {
                StatusWindowUpdater("Enter hostname to update...", false);
                await Task.Delay(2000);
                StatusWindowUpdater("STOP!!");
                return null;
            }

            var record = new Record
            {
                Domain = _dnsDomain,
                Type = "A",
                HostName = _dnsHost
            };

            return record;
        }

        private void DnsUpdaterReport(object? sender, StatusReport e)
        {
            StatusWindowUpdater(e.Content, e.Newline);
        }
        
        private void DnsUpdaterRrogress(object? sender, ProgressReport e)   
        {            
            if (!string.IsNullOrEmpty(e.Ip4)) 
            {
                CurrentV4iP = e.Ip4;
            }
        }


        private void StatusWindowUpdater(string logg, bool newLine = true)
        {

            var textToPrint = "";
            if (newLine) 
            { 
                textToPrint = Environment.NewLine;
                textToPrint += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": ";
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
