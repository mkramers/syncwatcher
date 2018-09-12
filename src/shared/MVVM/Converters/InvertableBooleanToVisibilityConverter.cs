using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MVVM.Converters
{
    /// <summary>
    ///     adapted from https://stackoverflow.com/a/915484
    ///     usage:
    ///     <Grid.Visibility>
    ///         <Binding Path="IsYesNoButtonSetVisible" Converter="{StaticResource booleanToVisibilityConverter}"
    ///             ConverterParameter="true" />
    ///     </Grid.Visibility>
    /// </summary>
    public sealed class InvertableBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object _value, Type _targetType, object _parameter, CultureInfo _culture)
        {
            bool flag = false;
            if (_value is bool b)
            {
                flag = b;
            }

            if (_parameter != null)
            {
                if (bool.Parse((string) _parameter))
                {
                    flag = !flag;
                }
            }

            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object _value, Type _targetType, object _parameter, CultureInfo _culture)
        {
            bool back = _value is Visibility visibility && visibility == Visibility.Visible;
            if (_parameter != null)
            {
                if ((bool) _parameter)
                {
                    back = !back;
                }
            }
            return back;
        }
    }
}
