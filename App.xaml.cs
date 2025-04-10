using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using PorkbunDnsUpdater.ViewModels;
using PorkbunDnsUpdater.Services;
using PorkbunDnsUpdater.Backend.PorkBun.WebClient;
using PorkbunDnsUpdater.Stores;
using PorkbunDnsUpdater.Models;

namespace PorkbunDnsUpdater
{
    public partial class App : System.Windows.Application
    {
               
        private readonly IServiceProvider _serviceProvider;
        private readonly NotifyIcon _notifyIcon;

        public App()
        {

            _notifyIcon = new NotifyIcon();

            IServiceCollection services = new ServiceCollection();

            
            services.AddSingleton<NavigationStore>();            
            services.AddSingleton<AppConfig>();

            services.AddSingleton<IPorkbunHttpClient, PorkbunHttpClient>();
            services.AddSingleton<PorkbunUpdaterService>(s => new PorkbunUpdaterService(s.GetRequiredService<IPorkbunHttpClient>()));

            services.AddSingleton<MainViewModel>(s => new MainViewModel(
                    s.GetRequiredService<NavigationStore>(),
                    CreateDnsUpdaterService(s)
                ));

            services.AddSingleton<MainWindow>(s => new MainWindow()
            {
                DataContext = s.GetRequiredService<MainViewModel>()
            });

            
            
            services.AddSingleton<INavigationService>(s => CreateDnsUpdaterService(s));
                        services.AddTransient<DnsUpdaterViewModel>(s => new DnsUpdaterViewModel(CreateDnsUpdaterService(s), s.GetRequiredService<AppConfig>(), s.GetRequiredService<PorkbunUpdaterService>()));
            
            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            StartUpNavigate(_serviceProvider).Navigate();
            
            MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                        
            _notifyIcon.Icon = new System.Drawing.Icon("Resources/dns.ico");
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "DNS Updater";
            MainWindow.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose();
            base.OnExit(e);
        }


        private IStartUpNavigate StartUpNavigate(IServiceProvider serviceProvider)
        {
            return new NavigationService<DnsUpdaterViewModel>(serviceProvider.GetRequiredService<NavigationStore>(),
                () => serviceProvider.GetRequiredService<DnsUpdaterViewModel>());
        }
        
        private INavigationService CreateDnsUpdaterService(IServiceProvider serviceProvider)
        {
            return new NavigationService<DnsUpdaterViewModel>(serviceProvider.GetRequiredService<NavigationStore>(),
                () => serviceProvider.GetRequiredService<DnsUpdaterViewModel>());
        }
    }
}
