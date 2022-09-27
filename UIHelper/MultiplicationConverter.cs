using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageCabinet.UIHelper
{
    public class MultiplicationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (UIHelper.TryParseDouble(value, out double a) && UIHelper.TryParseDouble(parameter, out double b))
            {
                return a * b;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
