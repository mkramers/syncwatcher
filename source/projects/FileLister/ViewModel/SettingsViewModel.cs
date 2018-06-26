using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Common.ViewModel;
using FileLister.Properties;
using GalaSoft.MvvmLight.Command;

namespace FileLister.ViewModel
{
    public class SettingsViewModel : ObservableObject
    {
        private void ClearInput()
        {
            Input = "";
        }

        private void AddIgnoredExtension(object _params)
        {
            var extension = _params as string;
            Debug.Assert(extension != null);

            extension = extension.ToLowerInvariant();

            var ignoredExtensions = new List<string>(Settings.Default.IgnoredExtensions);
            if (!ignoredExtensions.Contains(extension))
            {
                ignoredExtensions.Insert(0, extension);
                ClearInput();
            }

            Settings.Default.IgnoredExtensions = ignoredExtensions;
            Settings.Default.Save();
        }

        private void RemoveIgnoredExtensions(object _params)
        {
            List<string> extensions = null;

            var extension = _params as string;
            if (extension != null)
            {
                extensions = new List<string> {extension};
            }
            else
            {
                var items = (IList) _params;
                extensions = items.Cast<string>().ToList();
            }
            Debug.Assert(extensions != null);

            var ignoredExtensions = new List<string>(Settings.Default.IgnoredExtensions);
            foreach (var ext in extensions)
            {
                extension = ext.ToLowerInvariant();

                Debug.Assert(ignoredExtensions.Contains(extension));
                ignoredExtensions.Remove(extension);
            }

            Settings.Default.IgnoredExtensions = ignoredExtensions;
            Settings.Default.Save();
        }


        public ICommand AddIgnoredExtensionCommand => new RelayCommand<object>(AddIgnoredExtension);

        public ICommand RemoveIgnoredExtensionCommand => new RelayCommand<object>(RemoveIgnoredExtensions);

        public ICommand ClearInputCommand => new RelayCommand(ClearInput);

        public string Input
        {
            get => m_input;
            set
            {
                if (m_input != value)
                {
                    m_input = value;
                    RaisePropertyChangedEvent("Input");
                }
            }
        }

        private string m_input = "";
    }
}