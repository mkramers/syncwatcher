namespace MVVM.Popups.ViewModel
{
    public class InfoPopupViewModel : CloseableViewModel
    {
        public InfoPopupViewModel(string _message)
        {
            m_message = _message;
        }

        public string Message
        {
            get => m_message;
            set
            {
                m_message = value;
                RaisePropertyChanged();
            }
        }

        private string m_message;
    }
}