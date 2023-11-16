using CommunityToolkit.Mvvm.ComponentModel;

namespace NCMDumpGUI
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

        public NCMProcessStatus(string _path, string _status)
        {
            FilePath = _path;
            FileStatus = _status;
        }
    }
}