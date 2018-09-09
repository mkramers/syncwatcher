using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace Common.Framework
{
    //derived from https://www.codeproject.com/Articles/26288/Simplifying-the-WPF-TreeView-by-Using-the-ViewMode

    public class TreeViewItemViewModel : ViewModelBase
    {
        private static readonly TreeViewItemViewModel DummyChild = new TreeViewItemViewModel();

        private bool m_isExpanded;
        private bool m_isSelected;

        public bool HasDummyChild => Children.Count == 1 && Children[0] == DummyChild;

        public bool IsExpanded
        {
            get => m_isExpanded;
            set
            {
                if (value != m_isExpanded)
                {
                    m_isExpanded = value;
                    RaisePropertyChanged();
                }

                // Expand all the way up to the root.
                if (m_isExpanded && Parent != null)
                {
                    Parent.IsExpanded = true;
                }

                //this will trigger the children to update async. since the children are bound via observable collection, we can safely call async here
#pragma warning disable 4014
                LazyLoad();
#pragma warning restore 4014
            }
        }

        public bool IsSelected
        {
            get => m_isSelected;
            set
            {
                if (value != m_isSelected)
                {
                    m_isSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IEnumerable<TreeViewItemViewModel> SelectedChildren
        {
            get { return Children.Where(_child => _child.IsSelected); }
        }

        public ObservableCollection<TreeViewItemViewModel> Children { get; }
        public TreeViewItemViewModel Parent { get; }

        protected TreeViewItemViewModel(TreeViewItemViewModel _parent, bool _lazyLoadChildren)
        {
            Parent = _parent;

            Children = new ObservableCollection<TreeViewItemViewModel>();

            if (_lazyLoadChildren)
            {
                Children.Add(DummyChild);
            }
        }

        private TreeViewItemViewModel()
        {
        }

        protected virtual async Task LoadChildren()
        {
            await Task.FromResult(0);
        }

        public async Task LazyLoad()
        {
            // Lazy load the child items, if necessary.
            if (HasDummyChild)
            {
                Children.Remove(DummyChild);
                await LoadChildren();
            }
        }
    }
}
