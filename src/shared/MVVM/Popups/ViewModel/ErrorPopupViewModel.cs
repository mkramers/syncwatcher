namespace MVVM.Popups.ViewModel
{
    public class ErrorPopupViewModel : CloseableViewModel
    {
        public ErrorPopupViewModel(string _message)
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