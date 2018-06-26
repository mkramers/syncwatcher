using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FileGetter
{
    public enum FileObjectState
    {
        NONE,
        PENDING,
        IN_PROGRESS,
        COMPLETED,
        IGNORE,
        CANCELLED
    }

    public enum DirectoryObjectState
    {
        NONE,
        PENDING,
        IN_PROGRESS,
        COMPLETED,
        IGNORE,
        INCOMPLETE,
        CANCELLED
    }

    [ValueConversion(typeof(FileObjectState), typeof(SolidColorBrush))]
    public class FileObjectStateBrushConverter : IValueConverter
    {
        public object Convert(object _value, Type _targetType, object _parameter, CultureInfo _culture)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                if (!(_value is FileObjectState))
                    throw new ArgumentException("value not of type FileObjectState");

            SolidColorBrush color;

            var state = (FileObjectState) _value;
            switch (state)
            {
                case FileObjectState.NONE:
                    color = Brushes.Black;
                    break;
                case FileObjectState.PENDING:
                case FileObjectState.IN_PROGRESS:
                    color = Brushes.DodgerBlue;
                    break;
                case FileObjectState.COMPLETED:
                    color = Brushes.LawnGreen;
                    break;
                case FileObjectState.IGNORE:
                    color = Brushes.DarkGray;
                    break;
                case FileObjectState.CANCELLED:
                    color = Brushes.DarkOrange;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return color;
        }

        public object ConvertBack(object _value, Type _targetType, object _parameter, CultureInfo _culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(DirectoryObjectState), typeof(SolidColorBrush))]
    public class DirectoryObjectStateBrushConverter : IValueConverter
    {
        public object Convert(object _value, Type _targetType, object _parameter, CultureInfo _culture)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                if (!(_value is DirectoryObjectState))
                    throw new ArgumentException("value not of type DirectoryObjectState");

            if (_value == null)
                return Brushes.Yellow;

            SolidColorBrush color;

            var state = (DirectoryObjectState) _value;
            switch (state)
            {
                case DirectoryObjectState.NONE:
                case DirectoryObjectState.IGNORE:
                    color = Brushes.Black;
                    break;
                case DirectoryObjectState.PENDING:
                    color = Brushes.DarkGray;
                    break;
                case DirectoryObjectState.IN_PROGRESS:
                    color = Brushes.DodgerBlue;
                    break;
                case DirectoryObjectState.COMPLETED:
                    color = Brushes.LawnGreen;
                    break;
                case DirectoryObjectState.CANCELLED:
                    color = Brushes.DarkOrange;
                    break;
                case DirectoryObjectState.INCOMPLETE:
                    color = Brushes.MediumPurple;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return color;
        }

        public object ConvertBack(object _value, Type _targetType, object _parameter, CultureInfo _culture)
        {
            throw new NotImplementedException();
        }
    }
}