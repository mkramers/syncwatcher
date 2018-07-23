using System;

namespace Common.Framework.EventHelpers
{
    public class StringEventArgs : EventArgs
    {
        public StringEventArgs(string _value)
        {
            Value = _value;
        }

        public string Value { get; }   
    }
}
