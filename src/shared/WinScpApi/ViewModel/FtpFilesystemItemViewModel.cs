using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Framework;

namespace WinScpApi.ViewModel
{
    public abstract class FtpFilesystemItemViewModel : TreeViewItemViewModel
    {
        protected FtpFilesystemItemViewModel(TreeViewItemViewModel _parent, bool _lazyLoadChildren) : base(_parent,
            _lazyLoadChildren)
        {
        }

        public abstract FtpFilesystemItemViewModel Find(Func<FtpFilesystemItemViewModel, bool> _func);

        public abstract Task<IEnumerable<FtpFileViewModel>> GetSelectedFiles();

        public abstract string FullName { get; }
    }
}