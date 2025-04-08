using System.Windows.Input;
using PorkbunDnsUpdater.Commands;
using PorkbunDnsUpdater.Services;
using PorkbunDnsUpdater.Stores;

namespace PorkbunDnsUpdater.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;        
        private readonly INavigationService _DnsUpdaterNavigationService;
        
        public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;
        public ICommand DnsUpdaterCommand { get; internal set; }



        public MainViewModel(NavigationStore navigationStore,             
            INavigationService DnsUpdaterNavigationService
            )
        {
            _navigationStore = navigationStore;            
            _DnsUpdaterNavigationService = DnsUpdaterNavigationService;
            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;            
            DnsUpdaterCommand = new CommandBase(ExecuteDnsUpdaterCommand);
        }

       
        private void ExecuteDnsUpdaterCommand(object obj)
        {
            _DnsUpdaterNavigationService.Navigate();
        }


        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

    }
}
