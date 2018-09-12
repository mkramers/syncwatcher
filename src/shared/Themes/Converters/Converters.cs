using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Themes.Converters
{
    public class BoolToVisibilityConverter : BoolToValueConverter<Visibility>
    {
    }

    public class BoolToStringConverter : BoolToValueConverter<string>
    {
    }

    public class BoolToBrushConverter : BoolToValueConverter<Brush>
    {
    }

    public class BoolToObjectConverter : BoolToValueConverter<object>
    {
    }

    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { get; set; }
        public T TrueValue { get; set; }

        public object Convert(object _value, Type _targetType, object _parameter, CultureInfo _culture)
        {
            if (_value == null)
            {
                return FalseValue;
            }
            return (bool) _value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object _value, Type _targetType, object _parameter, CultureInfo _culture)
        {
            return _value?.Equals(TrueValue) ?? false;
        }
    }
}
