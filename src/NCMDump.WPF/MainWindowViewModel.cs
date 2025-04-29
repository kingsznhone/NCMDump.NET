using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NCMDump.Core;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace NCMDump.WPF
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly NCMDumper Dumper;
        private bool _isBusy = false;

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                SetProperty(ref _isBusy, value);
            }
        }

        private bool _willDeleteNCM;

        public bool WillDeleteNCM
        {
            get => _willDeleteNCM;
            set => SetProperty(ref _willDeleteNCM, value);
        }

        private string _applicationTitle;

        public string ApplicationTitle
        {
            get => _applicationTitle;
            set => SetProperty(ref _applicationTitle, value);
        }

        public ObservableCollection<NCMProcessStatus> NCMCollection { get; set; }

        public ObservableCollection<WindowBackdropType> BackdropCollection { get; set; }

        private WindowBackdropType _selectedBackdrop;

        public WindowBackdropType SelectedBackdrop
        {
            get { return _selectedBackdrop; }
            set
            {
                SetProperty(ref _selectedBackdrop, value);
                if ((App.Current as App).MainWindow != null)
                {
                    ((App.Current as App).MainWindow as FluentWindow).WindowBackdropType = value;
                }
            }
        }

        public IAsyncRelayCommand AddFolderCommand { get; }
        public IAsyncRelayCommand AddFileCommand { get; }
        public IAsyncRelayCommand ClearCommand { get; }
        public IAsyncRelayCommand ConvertCommand { get; }
        public IAsyncRelayCommand ThemeCommand { get; }

        public MainWindowViewModel(NCMDumper _core)
        {
            Dumper = _core;
            WillDeleteNCM = true;
            ApplicationTitle = "NCMDump.NET";
            NCMCollection = [];
            AddFolderCommand = new AsyncRelayCommand(OpenSelectFolderDialog);
            AddFileCommand = new AsyncRelayCommand(OpenSelectFileDialog);
            ClearCommand = new AsyncRelayCommand(ClearList, () => NCMCollection.Count > 0);
            ConvertCommand = new AsyncRelayCommand(StartConvert, () => NCMCollection.Count > 0);
            ThemeCommand = new AsyncRelayCommand(SwitchTheme);

            BackdropCollection = [
                WindowBackdropType.Auto,
                WindowBackdropType.Mica,
                WindowBackdropType.Acrylic,
                WindowBackdropType.None,
                WindowBackdropType.Tabbed
                ];

            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000, 0))
            {
                SelectedBackdrop = BackdropCollection.FirstOrDefault(x => x is WindowBackdropType.Mica);
            }
            else if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041, 0))
            {
                SelectedBackdrop = BackdropCollection.FirstOrDefault(x => x is WindowBackdropType.Acrylic);
            }
        }

        public void OnDrop(params string[] paths)
        {
            foreach (string path in paths)
            {
                if (new DirectoryInfo(path).Exists)
                {
                    WalkThrough(new DirectoryInfo(path));
                }
                else if (new FileInfo(path).Exists)
                {
                    if (path.EndsWith(@".ncm") && !NCMCollection.Any(x => x.FilePath == path))
                        NCMCollection.Add(new NCMProcessStatus(path, "Await"));
                }
            }
            ConvertCommand.NotifyCanExecuteChanged();
            ClearCommand.NotifyCanExecuteChanged();
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

        //
        private async Task StartConvert()
        {
            IsBusy = true;
            var dispatcher = Application.Current.Dispatcher;
            await Parallel.ForAsync(0, NCMCollection.Count, async (i, state) =>
            {
                if (NCMCollection[i].FileStatus != "Success")
                {
                    try
                    {
                        if (await Dumper.ConvertAsync(NCMCollection[i].FilePath))
                        {
                            await dispatcher.BeginInvoke(() => NCMCollection[i].FileStatus = "Success");
                            if (WillDeleteNCM)
                            {
                                try
                                {
                                    File.Delete(NCMCollection[i].FilePath);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.ToString());
                                }
                            }
                        }
                        else
                        {
                            await dispatcher.BeginInvoke(() => NCMCollection[i].FileStatus = "Failed");
                        }
                    }
                    catch
                    {
                        await dispatcher.BeginInvoke(() => NCMCollection[i].FileStatus = "Failed");
                    }
                }
            });

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive);
            GC.WaitForPendingFinalizers();
            IsBusy = false;
        }

        private async Task OpenSelectFolderDialog()
        {
            OpenFolderDialog ofp = new OpenFolderDialog
            {
                Multiselect = true,
            };

            if (ofp.ShowDialog() == true)
            {
                OnDrop(ofp.FolderNames);
            }
        }

        private async Task OpenSelectFileDialog()
        {
            OpenFileDialog ofp = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "NCM File(*.ncm)|*.ncm"
            };
            if (ofp.ShowDialog() == true)
            {
                OnDrop(ofp.FileNames);
            }
        }

        private async Task SwitchTheme()
        {
            var appTheme = ApplicationThemeManager.GetAppTheme();
            ApplicationTheme newTheme = appTheme == ApplicationTheme.Dark ? ApplicationTheme.Light : ApplicationTheme.Dark;

            WindowBackdropType backdrop = WindowBackdropType.Acrylic;
            if (newTheme == ApplicationTheme.Dark)
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000, 0))
                {
                    backdrop = WindowBackdropType.Mica;
                }
                else if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041, 0))
                {
                    backdrop = WindowBackdropType.Acrylic;
                }
                else
                {
                    backdrop = WindowBackdropType.None;
                }
            }
            ApplicationThemeManager.Apply(newTheme, backdrop);
        }

        private async Task ClearList()
        {
            NCMCollection.Clear();
            ConvertCommand.NotifyCanExecuteChanged();
            ClearCommand.NotifyCanExecuteChanged();
        }
    }
}