using System;

namespace Common.Framework.EventHelpers
{
    public class StringEventArgs : EventArgs
    {
        public string Value { get; }

        public StringEventArgs(string _value)
        {
            Value = _value;
        }
    }
}
