using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Common
{
    public class FileFilter : GroupFilter
    {
        public Filter ExtensionFilter { get; }
        public Filter StringFilter { get; }

        public FileFilter()
        {
            ExtensionFilter = new Filter(this);
            StringFilter = new Filter(this);
        }

        public void UpdateIgnoredExtensionsFilter(List<string> _ignoredExtensions)
        {
            Debug.Assert(_ignoredExtensions != null);

            ExtensionFilter.Set(
                item =>
                {
                    FileInfo file = item as FileInfo;
                    Debug.Assert(file != null);

                    return !_ignoredExtensions.Contains(Path.GetExtension(file.Name));
                });
        }
    }

    public class Filter
    {
        private readonly GroupFilter m_parent;

        private Predicate<object> m_filter;

        public Filter(GroupFilter _parent)
        {
            Debug.Assert(_parent != null);

            m_parent = _parent;
        }

        public void Set(Predicate<object> _filter)
        {
            Debug.Assert(_filter != null);

            Clear();

            m_filter = _filter;
            m_parent.AddFilter(m_filter);
        }

        public void Clear()
        {
            if (m_filter != null)
            {
                m_parent.RemoveFilter(m_filter);
            }
        }
    }
}
