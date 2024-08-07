﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using NCMDump.Core;
using NCMDump.WinUI.Models;
using Windows.Storage.Pickers;

namespace NCMDump.WinUI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly NCMDumper Core;
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

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
            set { SetProperty(ref _willDeleteNCM, value); Debug.WriteLine(WillDeleteNCM); }
        }

        private string _ApplicationTitle;

        public string ApplicationTitle
        {
            get => _ApplicationTitle;
            set => _ApplicationTitle = value;
        }

        private Visibility dropBoxVisible;

        public Visibility DropBoxVisible
        {
            get
            {
                return dropBoxVisible;
            }
            set
            {
                SetProperty(ref dropBoxVisible, value);
            }
        }

        public ObservableCollection<SystemBackdrop> BackdropCollection { get; set; }

        private SystemBackdrop _selectedBackdrop;

        public SystemBackdrop SelectedBackdrop
        {
            get { return _selectedBackdrop; }
            set
            {
                SetProperty(ref _selectedBackdrop, value);
                if ((App.Current as App).MainWindow != null)
                {
                    (App.Current as App).MainWindow.SystemBackdrop = value;
                }
            }
        }

        public ObservableCollection<NCMProcessStatus> NCMCollection { get; set; }

        public MainWindowViewModel(NCMDumper _core)
        {
            Core = _core;
            WillDeleteNCM = true;
            ApplicationTitle = "NCMDump.NET";
            DropBoxVisible = Visibility.Visible;
            NCMCollection = new ObservableCollection<NCMProcessStatus>();
            AddFolderCommand = new AsyncRelayCommand(FolderDialog);
            AddFileCommand = new AsyncRelayCommand(FileDialog);
            ClearCommand = new AsyncRelayCommand(ClearList, () => NCMCollection.Count > 0);
            ConvertCommand = new AsyncRelayCommand(StartConvert, () => NCMCollection.Count > 0);

            BackdropCollection = [new MicaBackdrop(), new DesktopAcrylicBackdrop()];

            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000, 0))
            {
                SelectedBackdrop = BackdropCollection.FirstOrDefault(x => x is MicaBackdrop);
            }
            else if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041, 0))
            {
                SelectedBackdrop = BackdropCollection.FirstOrDefault(x => x is DesktopAcrylicBackdrop);
            }
        }

        public void OnDrop(params string[] args)
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
                    {
                        DropBoxVisible = Visibility.Collapsed;
                        NCMCollection.Add(new NCMProcessStatus(_path, "Await"));
                    }
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
                {
                    DropBoxVisible = Visibility.Collapsed;
                    NCMCollection.Add(new NCMProcessStatus(f.FullName, "Await"));
                }
            }
        }

        public IAsyncRelayCommand AddFolderCommand { get; }
        public IAsyncRelayCommand AddFileCommand { get; }
        public IAsyncRelayCommand ClearCommand { get; }
        public IAsyncRelayCommand ConvertCommand { get; }

        private async Task StartConvert()
        {
            IsBusy = true;
            Debug.WriteLine("Clicked");
            await Parallel.ForAsync(0, NCMCollection.Count, async (i, state) =>
            {
                if (NCMCollection[i].FileStatus != "Success")
                {
                    try
                    {
                        if (await Core.ConvertAsync(NCMCollection[i].FilePath))
                        {
                            dispatcherQueue.TryEnqueue(() =>
                            {
                                NCMCollection[i].FileStatus = "Success";
                                NCMCollection[i].TextColor = new SolidColorBrush("Green".ToColor());
                            });

                            try
                            {
                                if (WillDeleteNCM)
                                {
                                    File.Delete(NCMCollection[i].FilePath);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
                            }
                        }
                        else
                        {
                            dispatcherQueue.TryEnqueue(() =>
                            {
                                NCMCollection[i].FileStatus = "Failed";
                                NCMCollection[i].TextColor = new SolidColorBrush("Red".ToColor());
                            });
                        }
                    }
                    catch
                    {
                        dispatcherQueue.TryEnqueue(() =>
                        {
                            NCMCollection[i].FileStatus = "Failed";
                            NCMCollection[i].TextColor = new SolidColorBrush("Red".ToColor());
                        });
                    }
                }
            });

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();

            IsBusy = false;

            int total = NCMCollection.Count;
            int success = NCMCollection.Count(x => x.FileStatus == "Success");
            //https://learn.microsoft.com/zh-cn/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/toast-notifications?tabs=appsdk
            var builder = new AppNotificationBuilder()
                .AddText($"Dumped! ")
                .AddText($"Success ({success}/{total})")
                .AddArgument("action", "clicked");

            AppNotificationManager.Default.Show(builder.BuildNotification());
        }

        private async Task FolderDialog()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle((App.Current as App).MainWindow);
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.ViewMode = PickerViewMode.Thumbnail;
            folderPicker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                OnDrop([folder.Path]);
            }
        }

        private async Task FileDialog()
        {
            //WinRT shithole

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle((App.Current as App).MainWindow);
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.FileTypeFilter.Add(".ncm");
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            var files = await filePicker.PickMultipleFilesAsync();
            if (files != null)
            {
                var filelist = files.Select(x => x.Path).ToArray();
                OnDrop(filelist);
            }
        }

        private async Task ClearList()
        {
            NCMCollection.Clear();
            DropBoxVisible = Visibility.Visible;
            ConvertCommand.NotifyCanExecuteChanged();
            ClearCommand.NotifyCanExecuteChanged();
        }
    }
}