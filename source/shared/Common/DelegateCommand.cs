using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Common.Mvvm
{
    /// <summary>
    /// Simplistic delegate command for the demo.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        public void Execute(object _parameter)
        {
            Debug.Assert(CommandAction != null);

            CommandAction();
        }

        public bool CanExecute(object _parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }
    }
}