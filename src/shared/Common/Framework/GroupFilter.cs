using System;
using System.Collections.Generic;

namespace Common
{
    public class GroupFilter
    {
        public GroupFilter()
        {
            m_filters = new List<Predicate<object>>();
            Filter = InternalFilter;
        }

        private bool InternalFilter(object o)
        {
            foreach (var filter in m_filters)
                if (!filter(o))
                    return false;

            return true;
        }

        public void AddFilter(Predicate<object> filter)
        {
            m_filters.Add(filter);

            TriggerEvent(this, new GroupFilterEventArgs(GroupFilterEvent.FILTER_MODIFIED));
        }

        public void RemoveFilter(Predicate<object> filter)
        {
            if (m_filters.Contains(filter))
            {
                m_filters.Remove(filter);

                TriggerEvent(this, new GroupFilterEventArgs(GroupFilterEvent.FILTER_MODIFIED));
            }
        }

        public void ClearFilters()
        {
            m_filters.Clear();

            TriggerEvent(this, new GroupFilterEventArgs(GroupFilterEvent.FILTER_MODIFIED));
        }

        private void TriggerEvent(object sender, GroupFilterEventArgs e)
        {
            Event?.Invoke(sender, e);
        }

        public event EventHandler<GroupFilterEventArgs> Event;

        public Predicate<object> Filter { get; }
        private readonly List<Predicate<object>> m_filters = new List<Predicate<object>>();
    }

    public class GroupFilterEventArgs : EventArgs
    {
        public GroupFilterEventArgs(GroupFilterEvent _type)
        {
            Type = _type;
        }

        public GroupFilterEvent Type { get; set; }
    }

    public enum GroupFilterEvent
    {
        NONE,
        FILTER_MODIFIED
    }
}