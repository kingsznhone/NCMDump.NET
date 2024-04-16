using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace NCMDumpGUI_WinUI.Models
{
    public partial class NCMProcessStatus : ObservableObject
    {
        private string filePath;

        public string FilePath
        {
            get => filePath;
            set => SetProperty(ref filePath, value);
        }

        public string _filestatus;

        public string FileStatus
        {
            get => _filestatus;
            set => SetProperty(ref _filestatus, value);
        }

        private SolidColorBrush textColor;

        public SolidColorBrush TextColor
        {
            get
            {
                if (FileStatus == "Success")
                {
                    var brush = new SolidColorBrush("Green".ToColor());
                    return brush;
                }
                else if (FileStatus == "Failed")
                {
                    var brush = new SolidColorBrush("Red".ToColor());
                    return brush;
                }
                else
                {
                    var brush = Application.Current.Resources["TextFillColorPrimaryBrush"] as SolidColorBrush;
                    return brush;
                }
            }
            set { SetProperty(ref textColor, value); }
        }

        public NCMProcessStatus(string _path, string _status)
        {
            textColor = Application.Current.Resources["TextFillColorPrimaryBrush"] as SolidColorBrush;
            FilePath = _path;
            FileStatus = _status;
        }
    }
}