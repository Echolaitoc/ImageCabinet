﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageCabinet.UIHelper
{
    public class AdditionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out double a) && double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double b))
            {
                return a + b;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
