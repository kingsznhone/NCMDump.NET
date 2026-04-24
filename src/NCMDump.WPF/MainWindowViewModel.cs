using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NCMDump.Core;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Controls;

namespace NCMDump.WPF
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly NCMDumper _dumper;
        private readonly IUiThreadDispatcher _dispatcher;

        private bool CanExecuteWhenNotBusy() => NCMCollection.Count > 0 && !IsBusy;

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
            NcmFileScanner.ScanPaths(paths, NCMCollection);
        }

        [RelayCommand(CanExecute = nameof(CanExecuteWhenNotBusy))]
        private async Task StartConvert(CancellationToken token)
        {
            IsBusy = true;
            try
            {
                // Snapshot items to avoid ObservableCollection thread safety issues
                var items = NCMCollection.Where(x => x.FileStatus != ConvertStatus.Success).ToArray();

                var options = new ParallelOptions
                {
                    CancellationToken = token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount * 2
                };

                await Parallel.ForAsync(0, items.Length, options, async (i, state) =>
                {
                    var item = items[i];
                    try
                    {
                        if (await _dumper.ConvertAsync(item.FilePath, cancellationToken: token))
                        {
                            await _dispatcher.InvokeAsync(() => item.FileStatus = ConvertStatus.Success);
                            if (WillDeleteNCM)
                            {
                                try
                                {
                                    File.Delete(item.FilePath);
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Delete error: {item.FilePath} - {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            await _dispatcher.InvokeAsync(() => item.FileStatus = ConvertStatus.Failed);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Convert error: {item.FilePath} - {ex}");
                        await _dispatcher.InvokeAsync(() => item.FileStatus = ConvertStatus.Failed);
                    }
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void SelectFolder()
        {
            OpenFolderDialog ofp = new()
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
            OpenFileDialog ofp = new()
            {
                Multiselect = true,
                Filter = "NCM File(*.ncm)|*.ncm"
            };
            if (ofp.ShowDialog() == true)
            {
                OnDrop(ofp.FileNames);
            }
        }

        [RelayCommand(CanExecute = nameof(CanExecuteWhenNotBusy))]
        private void ClearList() => NCMCollection.Clear();
    }
}
