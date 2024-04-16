using Microsoft.Extensions.DependencyInjection;
using NCMDumpCore;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace NCMDumpGUI
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;
        private ObservableCollection<NCMProcessStatus> NCMCollection = new();

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
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<NCMDumper>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();
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
                        NCMCollection.Add(new NCMProcessStatus(_path, "Await"));
                }
            }
        }

        private void WalkThrough(DirectoryInfo dir)
        {
            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                WalkThrough(d);
            }
            foreach (FileInfo f in dir.EnumerateFiles())
            {
                if (f.FullName.EndsWith(@".ncm") && !NCMCollection.Any(x => x.FilePath == f.FullName))
                    NCMCollection.Add(new NCMProcessStatus(f.FullName, "Await"));
            }
        }
    }
}