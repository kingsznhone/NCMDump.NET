using CommunityToolkit.Mvvm.ComponentModel;

namespace NCMDump.WPF
{
    public partial class NCMProcessStatus : ObservableObject
    {
        private string _filePath;

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        private string _filestatus;

        public string FileStatus
        {
            get => _filestatus;
            set => SetProperty(ref _filestatus, value);
        }

        public NCMProcessStatus(string path, string status)
        {
            FilePath = path;
            FileStatus = status;
        }
    }
}