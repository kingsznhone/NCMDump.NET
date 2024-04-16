using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.WindowsAPICodePack.Dialogs;
using NCMDumpCore;
using NCMDumpGUI_WinUI.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NCMDumpGUI_WinUI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly NCMDump Core;
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

        public ObservableCollection<NCMProcessStatus> NCMCollection { get; set; }

        public MainWindowViewModel(NCMDump _core)
        {
            Core = _core;
            WillDeleteNCM = true;
            ApplicationTitle = "NCMDump.NET";
            DropBoxVisible = Visibility.Visible;
            NCMCollection = new ObservableCollection<NCMProcessStatus>();
            AddFolderCommand = new RelayCommand(FolderDialog);
            AddFileCommand = new AsyncRelayCommand(FileDialog);
            ClearCommand = new RelayCommand(ClearList);
            ConvertCommand = new AsyncRelayCommand(StartConvert);
        }

        public void OnDrop(string[] args)
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

        public ICommand AddFolderCommand { get; }
        public IAsyncRelayCommand AddFileCommand { get; }
        public ICommand ClearCommand { get; }
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
        }

        private void FolderDialog()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string folderPath = dialog.FileName;
                OnDrop([folderPath]);
            }
        }

        private async Task FileDialog()
        {
            Microsoft.Win32.OpenFileDialog ofp = new Microsoft.Win32.OpenFileDialog();
            ofp.Multiselect = true;
            ofp.Filter = "NCM File(*.ncm)|*.ncm";
            if (ofp.ShowDialog() == true)
            {
                OnDrop(ofp.FileNames);
            }

            //WinRT shithole

            //var hwnd = WinRT.Interop.WindowNative.GetWindowHandle((App.Current as App).MainWindow);
            //FileOpenPicker filePicker = new FileOpenPicker();
            //filePicker.ViewMode = PickerViewMode.List;
            //filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            //filePicker.FileTypeFilter.Add("*");
            //WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            //var files = await filePicker.PickMultipleFilesAsync();
            //if (files != null)
            //{
            //    var filelist = files.Select(x => x.Path).ToArray();
            //    OnDrop(filelist);
            //}
        }

        private void ClearList()
        {
            NCMCollection.Clear();
            DropBoxVisible = Visibility.Visible;
        }
    }
}