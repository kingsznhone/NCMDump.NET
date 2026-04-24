using Microsoft.Extensions.DependencyInjection;
using NCMDump.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace NCMDump.WPF
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;
        private readonly ObservableCollection<NCMConvertMissionStatus> _ncmCollection = [];

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            NcmFileScanner.ScanPaths([.. e.Args], _ncmCollection);

            if (_ncmCollection.Count > 0)
            {
                MainWindowViewModel vm = _serviceProvider.GetRequiredService<MainWindowViewModel>();
                vm.NCMCollection = _ncmCollection;
            }
            MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            MainWindow.Show();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<NCMDumper>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<IUiThreadDispatcher>(_ => new WpfUiThreadDispatcher(Current.Dispatcher));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            base.OnExit(e);
        }
    }
}
