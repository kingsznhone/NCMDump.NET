using Microsoft.Extensions.DependencyInjection;
using NCMDump.Core;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace NCMDump.WPF
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;
        private ObservableCollection<NCMConvertMissionStatus> NCMCollection = new();
        private int depth;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            OnDrop(e.Args.ToArray());
            if (NCMCollection.Count > 0)
            {
                MainWindowViewModel vm = _serviceProvider.GetRequiredService<MainWindowViewModel>();
                vm.NCMCollection = NCMCollection;
            }
            MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            MainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
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

        private void OnDrop(string[] args)
        {
            foreach (string _path in args)
            {
                if (new DirectoryInfo(_path).Exists)
                {
                    WalkThrough(new DirectoryInfo(_path));
                }
                else if (new FileInfo(_path).Exists)
                {
                    if (_path.EndsWith(@".ncm") && !NCMCollection.Any(x => x.FilePath == _path))
                        NCMCollection.Add(new NCMConvertMissionStatus(_path, "Await"));
                }
            }
        }

        private void WalkThrough(DirectoryInfo dir)
        {
            depth++;
            if (depth > 16)
            {
                depth--;
                return;
            }
            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                WalkThrough(d);
            }
            foreach (FileInfo f in dir.EnumerateFiles())
            {
                if (f.FullName.EndsWith(@".ncm") && !NCMCollection.Any(x => x.FilePath == f.FullName))
                    NCMCollection.Add(new NCMConvertMissionStatus(f.FullName, "Await"));
            }
            depth--;
        }
    }
}