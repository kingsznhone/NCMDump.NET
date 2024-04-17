using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Composition.SystemBackdrops;
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
            SystemBackdrop = VM.SelectedBackdrop;
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            theme = new UISettings();
            theme.ColorValuesChanged += UISettings_ColorValuesChanged;

            this.InitializeComponent();
            var datagrid = WorkingList;
            RootGrid.DataContext = VM;
        }

        private void SetWindowPosition()
        {
            if (AppWindow is not null)
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32 { Height = 800, Width = 1080 });
                Microsoft.UI.Windowing.DisplayArea displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(this.AppWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);
                if (displayArea is not null)
                {
                    var CenteredPosition = AppWindow.Position;
                    CenteredPosition.X = ((displayArea.WorkArea.Width - AppWindow.Size.Width) / 2);
                    CenteredPosition.Y = ((displayArea.WorkArea.Height - AppWindow.Size.Height) / 2);
                    AppWindow.Move(CenteredPosition);
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

        private void WorkingList_Loaded(object sender, RoutedEventArgs e)
        {
            WorkingList.Columns[0].Width = new DataGridLength(4, DataGridLengthUnitType.Star);
            WorkingList.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }

        private void DataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            WorkingList.Columns[0].Width = new DataGridLength(4, DataGridLengthUnitType.Star);
            WorkingList.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }

        private void Window_Closed(object sender, WindowEventArgs e)
        {
            App.Current.Exit();
        }
    }
}