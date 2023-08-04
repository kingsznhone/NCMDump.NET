using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NCMDumpGUI
{
    [ObservableObject]
    public partial class MainWindowViewModel
    {
        private bool _willDeleteNCM;

        public bool WillDeleteNCM
        {
            get
            {
                return _willDeleteNCM;
            }
            set
            {
                SetProperty(ref _willDeleteNCM, value);
                Debug.Assert(WillDeleteNCM == value);
            }
        }

        public MainWindowViewModel()
        {
            WillDeleteNCM = true;
        }
    }
}