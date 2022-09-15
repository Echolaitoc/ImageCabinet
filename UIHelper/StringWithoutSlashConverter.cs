using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageCabinet.UIHelper
{
    public class StringWithoutSlashConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                str = str.Replace(@"/", string.Empty);
                str = str.Replace(@"\", string.Empty);
                return str;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
