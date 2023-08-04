using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using HandyControl.Controls;
using NCMDumpCore;

namespace NCMDumpGUI
{
    public enum ACCENTSTATE
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
        ACCENT_INVALID_STATE = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ACCENTPOLICY
    {
        public ACCENTSTATE AccentState;
        public int AccentFlags;
        public uint GradientColor;
        public int AnimationId;
    }

    public enum WINDOWCOMPOSITIONATTRIB
    {
        WCA_ACCENT_POLICY = 19
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINCOMPATTRDATA
    {
        public WINDOWCOMPOSITIONATTRIB Attribute;
        public IntPtr Data;
        public int DataSize;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BlurWindow
    {
        private NCMDump Core = new NCMDump();

        [DllImport("user32.dll")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WINCOMPATTRDATA data);

        public ObservableCollection<NCMProcessStatus> NCMCollection { get; set; }
         = new ObservableCollection<NCMProcessStatus>();

        private MainWindowViewModel VM;

        public MainWindow()
        {
            VM = new MainWindowViewModel();
            this.DataContext = VM;
            InitializeComponent();
            WorkingList.ItemsSource = NCMCollection;
            App.Current.Resources["BlurGradientValue"] = 0xaaffffff;
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            System.Windows.Controls.ListView listView = sender as System.Windows.Controls.ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth; // take into account vertical scrollbar
            var col1 = 0.60;
            var col2 = 0.20;
            var col3 = 0.20;
            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
            gView.Columns[2].Width = workingWidth * col3;
        }

        private void WorkingList_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string[] args = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            if (args is not null && args.Length != 0)
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

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ParallelLoopResult result = Parallel.For(0, NCMCollection.Count, async (i, state) =>
            {
                if (NCMCollection[i].FileStatus != "Success")
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    sw.Start();
                    if (await Core.ConvertAsync(NCMCollection[i].FilePath))
                    {
                        sw.Stop();
                        NCMCollection[i].Elapsedms = $"{sw.ElapsedMilliseconds:f0}ms";
                        NCMCollection[i].FileStatus = "Success";
                        Dispatcher.Invoke(() => this.UpdateLayout());
                        try
                        {
                            if (VM.WillDeleteNCM)
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
                        sw.Stop();
                        NCMCollection[i].Elapsedms = $"{sw.ElapsedMilliseconds:f0}ms";
                        NCMCollection[i].FileStatus = "Failed";
                        Dispatcher.Invoke(() => this.UpdateLayout());
                    }
                }
            });

            if (result.IsCompleted)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();
            }
            else
            {
                Debug.WriteLine("Paralle Loop Not Complete.");
            }
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofp = new Microsoft.Win32.OpenFileDialog();
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

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string folderPath = dialog.SelectedPath;
                    WalkThrough(new DirectoryInfo(folderPath));
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            NCMCollection.Clear();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}