using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageCabinet.UIHelper
{
    public class DoubleToSizeConverter : IValueConverter
    {
        public double Width { get; set; } = double.NaN;
        public double Height { get; set; } = double.NaN;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var width = Width;
            var height = Height;
            if (value is double d)
            {
                width = double.IsNaN(Width) ? d : Width;
                height = double.IsNaN(Height) ? d : Height;
            }
            if (double.IsNaN(width) || double.IsNaN(height))
            {
                return DependencyProperty.UnsetValue;
            }    
            return new Size(width, height);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
