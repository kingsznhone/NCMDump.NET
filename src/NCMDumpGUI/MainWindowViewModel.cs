using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using NCMDumpCore;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace NCMDumpGUI
{
    [ObservableObject]
    public partial class MainWindowViewModel
    {
        private readonly NCMDumper Core;
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

        public MainWindowViewModel(NCMDumper _core)
        {
            Core = _core;
            WillDeleteNCM = true;
            ApplicationTitle = "NCMDump.NET";
            NCMCollection = new();
            AddFolderCommand = new RelayCommand(FolderDialog);
            AddFileCommand = new RelayCommand(FileDialog);
            ClearCommand = new RelayCommand(ClearList);
            ConvertCommand = new AsyncRelayCommand(StartConvert);
            ThemeCommand = new RelayCommand(SwitchTheme);
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

        public ICommand AddFolderCommand { get; }
        public ICommand AddFileCommand { get; }
        public ICommand ClearCommand { get; }
        public IAsyncRelayCommand ConvertCommand { get; }
        public ICommand ThemeCommand { get; }

        private async Task StartConvert()
        {
            IsBusy = true;
            await Parallel.ForAsync(0, NCMCollection.Count, async (i, state) =>
            {
                if (NCMCollection[i].FileStatus != "Success")
                {
                   // try
                    {
                        if (await Core.ConvertAsync(NCMCollection[i].FilePath))
                        {
                            NCMCollection[i].FileStatus = "Success";
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
                            NCMCollection[i].FileStatus = "Failed";
                        }
                    }
                    //catch
                    //{
                    //    NCMCollection[i].FileStatus = "Failed";
                    //}
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
                IsFolderPicker = true,
                Title = "选择文件夹"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string folderPath = dialog.FileName;
                OnDrop([folderPath]);
            }
        }

        private void FileDialog()
        {
            Microsoft.Win32.OpenFileDialog ofp = new();
            ofp.Multiselect = true;
            ofp.Filter = "NCM File(*.ncm)|*.ncm";
            if (ofp.ShowDialog() == true)
            {
                foreach (string file in ofp.FileNames)
                {
                    if (file.EndsWith(@".ncm") && !NCMCollection.Any(x => x.FilePath == file))
                        NCMCollection.Add(new NCMProcessStatus(file, "Await"));
                }
            }
        }

        private void SwitchTheme()
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

        private void ClearList() => NCMCollection.Clear();
    }
}