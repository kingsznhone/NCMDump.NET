using Microsoft.Win32;
using NCMDumpCore;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace NCMDumpGUI
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NCMDump core = new NCMDump();

        public ObservableCollection<NCMProcessStatus> NCMCollection { get; set; }
         = new ObservableCollection<NCMProcessStatus>();

        public MainWindow()
        {
            InitializeComponent();
            //WorkingList.Items.Add(new NCMProcessStatus ( "AAAAA", "Done"));
            WorkingList.ItemsSource = NCMCollection;
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            System.Windows.Controls.ListView listView = sender as System.Windows.Controls.ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth; // take into account vertical scrollbar
            var col1 = 0.80;
            var col2 = 0.20;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
        }

        private void WorkingList_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string[] args = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            if (args != null && args.Length != 0)
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
            for (int i = 0; i < NCMCollection.Count; i++)
            {
                if (NCMCollection[i].FileStatus == "Success") continue;

                if (await Task.Run(() => core.ConvertAsync(NCMCollection[i].FilePath)))
                {
                    NCMCollection[i].FileStatus = "Success";
                    this.UpdateLayout();
                }
                else
                {
                    NCMCollection[i].FileStatus = "Fail";
                    this.UpdateLayout();
                }

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
