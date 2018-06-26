using System;

namespace P4Commands
{
    public static class Extensions
    {
        public static T[] SubArray<T>(this T[] _data, int _index, int _length)
        {
            var result = new T[_length];
            Array.Copy(_data, _index, result, 0, _length);
            return result;
        }
    }
}