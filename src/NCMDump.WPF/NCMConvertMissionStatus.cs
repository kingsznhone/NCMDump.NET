using CommunityToolkit.Mvvm.ComponentModel;

namespace NCMDump.WPF
{
    public partial class NCMConvertMissionStatus : ObservableObject
    {
        public string FilePath
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public string FileStatus
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public NCMConvertMissionStatus(string path, string status)
        {
            FilePath = path;
            FileStatus = status;
        }
    }
}