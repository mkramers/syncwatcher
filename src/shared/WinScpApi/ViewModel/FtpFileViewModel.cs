using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Common.SFTP;

namespace WinScpApi.ViewModel
{
    public class FtpFileViewModel : FtpFilesystemItemViewModel
    {
        public FileObject File { get; }

        public override string FullName => File.FullName;

        public FtpFileViewModel(FileObject _file, FtpDirectoryViewModel _parent) : base(_parent, false)
        {
            Debug.Assert(_file != null);

            File = _file;
            File.PropertyChanged += File_PropertyChanged;
        }

        private void File_PropertyChanged(object _sender, PropertyChangedEventArgs _e)
        {
            string changedProperty = _e?.PropertyName;
            Debug.Assert(!string.IsNullOrWhiteSpace(changedProperty));

            if (changedProperty == nameof(File.State))
            {
                if (Parent is FtpDirectoryViewModel parentDirectory)
                {
                    parentDirectory.RefreshState();
                }
            }
        }

        public override FtpFilesystemItemViewModel Find(Func<FtpFilesystemItemViewModel, bool> _func)
        {
            FtpFilesystemItemViewModel match = null;
            if (_func(this))
            {
                match = this;
            }
            return match;
        }

        public override async Task<IEnumerable<FtpFileViewModel>> GetSelectedFiles()
        {
            List<FtpFileViewModel> files = new List<FtpFileViewModel>(1);
            await Task.Factory.StartNew(
                () =>
                {
                    if (IsSelected)
                    {
                        files.Add(this);
                    }
                });
            return files;
        }
    }
}
