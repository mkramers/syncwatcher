namespace MVVM.Popups.ViewModel
{
    public class ErrorPopupViewModel : CloseableViewModel
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

        public ErrorPopupViewModel(string _message)
        {
            m_message = _message;
        }
    }
}
