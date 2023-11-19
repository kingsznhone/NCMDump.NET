using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using NCMDumpGUI_WinUI.ViewModels;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.ViewManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCMDumpGUI_WinUI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct tagPOINT
    {
        public long x;
        public long y;
    }

    // 定义MINMAXINFO结构体
    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public tagPOINT ptReserved;
        public tagPOINT ptMaxSize;
        public tagPOINT ptMaxPosition;
        public tagPOINT ptMinTrackSize;
        public tagPOINT ptMaxTrackSize;
    }

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private MainWindowViewModel VM;
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        private UISettings theme;

        public MainWindow(MainWindowViewModel _vm)
        {
            VM = _vm;
            SetWindowPosition();
            this.InitializeComponent();
            this.RootGrid.DataContext = VM;
            this.SystemBackdrop = new DesktopAcrylicBackdrop();
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
            var datagrid = WorkingList;
            theme = new UISettings();
            theme.ColorValuesChanged += UISettings_ColorValuesChanged;
        }

        private void SetWindowPosition()
        {
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Height = 720, Width = 960 });

            if (this.AppWindow is not null)
            {
                Microsoft.UI.Windowing.DisplayArea displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(this.AppWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);
                if (displayArea is not null)
                {
                    var CenteredPosition = this.AppWindow.Position;
                    CenteredPosition.X = ((displayArea.WorkArea.Width - this.AppWindow.Size.Width) / 2);
                    CenteredPosition.Y = ((displayArea.WorkArea.Height - this.AppWindow.Size.Height) / 2);
                    this.AppWindow.Move(CenteredPosition);
                }
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Open";
        }

        private async void WorkingList_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    string[] args = items.Select(x =>
                    {
                        if (x is StorageFile)
                        {
                            return (x as StorageFile).Path;
                        }
                        else if (x is StorageFolder)
                        {
                            return (x as StorageFolder).Path;
                        }
                        else
                        {
                            return "";
                        }
                    }).ToArray();
                    VM.OnDrop(args);
                }
            }

            //
        }

        private void UISettings_ColorValuesChanged(UISettings sender, object args)
        {
            dispatcherQueue.TryEnqueue(() =>
                {
                    var data = WorkingList.DataContext;
                    WorkingList.DataContext = null;
                    WorkingList.DataContext = data;
                });
        }

        private void DataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            var workingWidth = grid.ActualWidth;
            grid.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            grid.Columns[1].Width = new DataGridLength(120);
        }

        private void Window_Closed(object sender, WindowEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}