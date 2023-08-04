using System.ComponentModel;

namespace NCMDumpGUI
{
    public class NCMProcessStatus : INotifyPropertyChanged
    {
        public string FilePath { get; set; }

        public string _filestatus;

        private string _elapsedms;

        public string Elapsedms
        {
            get { return _elapsedms; }
            set { _elapsedms = value; OnPropertyChanged("Elapsedms"); }
        }

        public string FileStatus
        {
            get { return _filestatus; }
            set { _filestatus = value; OnPropertyChanged("FileStatus"); }
        }

        public NCMProcessStatus(string _path, string _status)
        {
            FilePath = _path;
            FileStatus = _status;
            Elapsedms = "";
        }

        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged is not null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}