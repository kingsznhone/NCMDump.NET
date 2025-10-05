using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NCMDump.Core;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Controls;

namespace NCMDump.WPF
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly NCMDumper _dumper;
        private readonly IUiThreadDispatcher _dispatcher;

        private bool ClearListCanExecute() => NCMCollection.Count > 0 && !IsBusy;

        private bool StartConvertCanExecute() => NCMCollection.Count > 0 && !IsBusy;

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial bool WillDeleteNCM { get; set; }

        [ObservableProperty]
        public partial string ApplicationTitle { get; set; }

        public ObservableCollection<NCMConvertMissionStatus> NCMCollection { get; set; }
        public ObservableCollection<WindowBackdropType> BackdropCollection { get; set; }

        public WindowBackdropType SelectedBackdrop
        {
            get => field;
            set
            {
                SetProperty(ref field, value);
                if (Application.Current.MainWindow != null)
                {
                    (Application.Current.MainWindow as FluentWindow)!.WindowBackdropType = value;
                }
            }
        }

        public MainWindowViewModel(NCMDumper dumper, IUiThreadDispatcher dispatcher)
        {
            _dumper = dumper;
            _dispatcher = dispatcher;
            WillDeleteNCM = true;
            ApplicationTitle = "NCMDump.NET";
            NCMCollection = [];
            NCMCollection.CollectionChanged += (_, _) =>
            {
                ClearListCommand.NotifyCanExecuteChanged();
                StartConvertCommand.NotifyCanExecuteChanged();
            };
            BackdropCollection = [
                WindowBackdropType.Auto,
                WindowBackdropType.Mica,
                WindowBackdropType.Acrylic,
                WindowBackdropType.None,
                WindowBackdropType.Tabbed
                ];

            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000, 0))
            {
                SelectedBackdrop = WindowBackdropType.Mica;
            }
            else if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041, 0))
            {
                SelectedBackdrop = WindowBackdropType.Acrylic;
            }
            else
            {
                SelectedBackdrop = WindowBackdropType.Auto;
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
                        NCMCollection.Add(new NCMConvertMissionStatus(path, "Await"));
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
                    NCMCollection.Add(new NCMConvertMissionStatus(f.FullName, "Await"));
            }
        }

        [RelayCommand(CanExecute = nameof(StartConvertCanExecute))]
        private async Task StartConvert()
        {
            IsBusy = true;
            await Parallel.ForAsync(0, NCMCollection.Count, async (i, state) =>
            {
                if (NCMCollection[i].FileStatus != "Success")
                {
                    try
                    {
                        if (await _dumper.ConvertAsync(NCMCollection[i].FilePath))
                        {
                            await _dispatcher.InvokeAsync(() => NCMCollection[i].FileStatus = "Success");
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
                            await _dispatcher.InvokeAsync(() => NCMCollection[i].FileStatus = "Failed");
                        }
                    }
                    catch (Exception ex)
                    {
                        await _dispatcher.InvokeAsync(() => NCMCollection[i].FileStatus = "Failed");
                    }
                }
            });
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive);
            GC.WaitForPendingFinalizers();
            IsBusy = false;
        }

        [RelayCommand]
        private void SelectFolder()
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

        [RelayCommand]
        private void SelectFile()
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

        [RelayCommand(CanExecute = nameof(ClearListCanExecute))]
        private void ClearList() => NCMCollection.Clear();
    }
}