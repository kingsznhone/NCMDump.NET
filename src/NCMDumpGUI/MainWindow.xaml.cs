using System;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace NCMDumpGUI
{
    public partial class MainWindow : FluentWindow
    {
        private MainWindowViewModel VM;

        public MainWindow(MainWindowViewModel _vm)
        {
            VM = _vm;
            DataContext = VM;
            WindowBackdropType = VM.SelectedBackdrop;
            SystemThemeWatcher.Watch(this,VM.SelectedBackdrop,false);
            InitializeComponent();
            
        }

        private void DataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            var workingWidth = grid.ActualWidth - SystemParameters.VerticalScrollBarWidth;
            grid.Columns[0].Width = workingWidth - 130;
            grid.Columns[1].Width = 120;
        }

        private void WorkingList_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string[] args = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (args is not null && args.Length != 0)
            {
                VM.OnDrop(args);
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}