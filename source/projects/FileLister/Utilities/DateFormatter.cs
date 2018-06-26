using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace FileLister.Utilities
{
    public class DateFormatter : IValueConverter
    {
        // This converts the DateTime object to the string to display.
        public object Convert(object _value, Type _targetType,
            object _parameter, CultureInfo _language)
        {
            //// Retrieve the format string and use it to format the value.
            //string formatString = parameter as string;
            //if (!string.IsNullOrEmpty(formatString))
            //{
            //    return string.Format(
            //        language, formatString, value);
            //}
            //// If the format string is null or empty, simply call ToString()
            //// on the value.
            //return value.ToString();
            var fileInfos = _value as IEnumerable<FileInfo>;
            Debug.Assert(fileInfos != null);
            return "";
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object _value, Type _targetType,
            object _parameter, CultureInfo _language)
        {
            throw new NotImplementedException();
        }
    }
}