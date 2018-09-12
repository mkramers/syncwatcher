namespace MVVM.Popups.ViewModel
{
    public class InfoPopupViewModel : CloseableViewModel
    {
        private string m_message;

        public string Message
        {
            get => m_message;
            set
            {
                m_message = value;
                RaisePropertyChanged();
            }
        }

        public InfoPopupViewModel(string _message)
        {
            m_message = _message;
        }
    }
}
