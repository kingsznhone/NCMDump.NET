using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Linq;
using NCMDumpCore;
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
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth; // take into account vertical scrollbar
            var col1 = 0.80;
            var col2 = 0.20;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
        }

        private void WorkingList_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length != 0)
            {
                foreach (string file in files)
                {
                    if (file.EndsWith(@".ncm") && !NCMCollection.Any(x=>x.FilePath==file))
                        NCMCollection.Add(new NCMProcessStatus(file, "Await"));
                }
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < NCMCollection.Count; i++)
            {
                if (NCMCollection[i].FileStatus == "Success") continue;

                if (await core.ConvertAsync(NCMCollection[i].FilePath))
                {
                    NCMCollection[i].FileStatus = "Success";
                }
                else
                {
                    NCMCollection[i].FileStatus = "Fail";
                }

            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofp =     new OpenFileDialog();
            ofp.Multiselect = true;
            ofp.Filter = "NCM File(*.ncm)|*.ncm";

            if (ofp.ShowDialog() == true)
            {
                foreach(string file in ofp.FileNames)
                {
                    if (file.EndsWith(@".ncm") && !NCMCollection.Any(x => x.FilePath == file))
                        NCMCollection.Add(new NCMProcessStatus(file, "Await"));
                }
            }
        }
    }
}
