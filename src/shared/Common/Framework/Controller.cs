using System.Diagnostics;

namespace Common.Framework
{
    public abstract class Controller
    {
        public Model Model { get; set; }
        public View View { get; set; }

        public Controller(Model _model, View _view)
        {
            Debug.Assert(_model != null);
            Debug.Assert(_view != null);

            Model = _model;

            View = _view;
            View.ViewEvent += OnViewEvent;
        }

        protected abstract void OnViewEvent(object sender, ViewEventArgs e);
    }
}
