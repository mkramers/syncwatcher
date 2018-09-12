using System;
using System.Globalization;
using System.Windows.Data;

namespace Themes.Converters
{
    [ValueConversion(typeof(long), typeof(string))]
    public class FileSizeToStringConverter : IValueConverter
    {
        public object Convert(object _value, Type _targetType, object _parameter, CultureInfo _culture)
        {
            if (!(_value is long))
            {
                throw new ArgumentException("value not of type int");
            }

            long value = (long) _value;

            float sizeKb = (float) value / 1000;
            float sizeMb = sizeKb / 1000;

            string text = sizeMb.ToString("n2");
            return text;
        }

        public object ConvertBack(object _value, Type _targetType, object _parameter, CultureInfo _language)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object _value, Type _targetType, object _parameter, CultureInfo _culture)
        {
            if (_targetType != typeof(bool))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }

            return _value != null && !(bool) _value;
        }

        public object ConvertBack(object _value, Type _targetType, object _parameter, CultureInfo _culture)
        {
            throw new NotSupportedException();
        }
    }
}
