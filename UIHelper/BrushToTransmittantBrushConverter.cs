using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ImageCabinet.UIHelper
{
    public class BrushToTransmittantBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double opacity)) return DependencyProperty.UnsetValue;
            bool inputValid = false;
            Color color;
            if (value is SolidColorBrush brush)
            {
                color = brush.Color;
                if (brush.Opacity != 1.0)
                {
                    color.A = (byte)(color.A * brush.Opacity);
                }
                inputValid = true;
            }
            else if (value is Color sourceColor)
            {
                color = sourceColor;
                inputValid = true;
            }
            if (inputValid)
            {
                color.A = (byte)(color.A * opacity);
                return new SolidColorBrush(color);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
