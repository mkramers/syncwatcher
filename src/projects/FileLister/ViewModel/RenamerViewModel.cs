using GalaSoft.MvvmLight;

namespace FileLister.ViewModel
{
    public class RenamerViewModel : ViewModelBase
    {
        public ObservableRangeCollection<string> InputItems
        {
            get => m_inputItems;
            set
            {
                m_inputItems = value;

                RaisePropertyChanged("InputItems");
            }
        }

        private ObservableRangeCollection<string> m_inputItems = new ObservableRangeCollection<string>();
    }
}