using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageCabinet.UIHelper
{
    public class CultureAgnosticDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DoConversion(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DoConversion(value);
        }

        private object DoConversion(object value)
        {
            if (value is double d) return d;
            if (UIHelper.TryParseDouble(value, out double doubleValue))
            {
                return doubleValue;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
