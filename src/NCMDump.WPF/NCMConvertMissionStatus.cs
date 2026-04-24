using CommunityToolkit.Mvvm.ComponentModel;

namespace NCMDump.WPF
{
    /// <summary>
    /// Defines constants for NCM convert status values used in UI bindings.
    /// </summary>
    public static class ConvertStatus
    {
        public const string Await = "Await";
        public const string Success = "Success";
        public const string Failed = "Failed";
    }

    public partial class NCMConvertMissionStatus : ObservableObject
    {
        [ObservableProperty]
        public partial string FilePath { get; set; }

        [ObservableProperty]
        public partial string FileStatus { get; set; }

        public NCMConvertMissionStatus(string path, string status)
        {
            FilePath = path;
            FileStatus = status;
        }
    }
}
