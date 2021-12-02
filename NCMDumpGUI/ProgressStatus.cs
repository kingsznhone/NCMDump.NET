using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCMDumpGUI
{ 
    public class NCMProcessStatus: INotifyPropertyChanged
    {
        public string FilePath { get; set; }

        public string _filestatus;
        public string FileStatus {
            get { return _filestatus; }
            set { _filestatus = value; OnPropertyChanged("FileStatus"); }
        }
        public NCMProcessStatus(string _path,string _status)
        {
            FilePath = _path;
            FileStatus = _status;
        }

        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
