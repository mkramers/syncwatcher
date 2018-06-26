using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Common.Framework
{
    public class View : Page
    {
        protected void TriggerViewEvent(ViewEventArgs _eventArgs)
        {
            Console.WriteLine("POOP");
            ViewEvent?.Invoke(this, _eventArgs);
        }

        public event EventHandler<ViewEventArgs> ViewEvent;
    }

    public class ViewEventArgs : RoutedEventArgs
    {
    }

    public class FileViewEventArgs : ViewEventArgs
    {
        public FileViewEventArgs(FileViewEvents _type)
        {
            Type = _type;
        }

        public FileViewEvents Type { get; }
    }

    public class Search_FileViewEventArgs : FileViewEventArgs
    {
        public Search_FileViewEventArgs(FileViewEvents _type, string _search) : base(_type)
        {
            Search = _search;
        }

        public string Search { get; set; }
    }

    public class ListBoxSelectionChanged_FileViewEventArgs : FileViewEventArgs
    {
        public ListBoxSelectionChanged_FileViewEventArgs(FileViewEvents _type, IList _selected) : base(_type)
        {
            Selected = _selected;
        }

        public IList Selected { get; set; }
    }

    public enum FileViewEvents
    {
        NONE,
        SEARCH_CHANGED,
        LIST_SELECTION_CHANGED,
        DATAGRID_REFRESHED
    }
}