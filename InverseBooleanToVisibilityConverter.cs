using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageCabinet
{
    internal class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool b) return DependencyProperty.UnsetValue;

            if (b) return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Visibility vis) return DependencyProperty.UnsetValue;

            if (vis == Visibility.Visible) return false;

            return true;
        }
    }
}
