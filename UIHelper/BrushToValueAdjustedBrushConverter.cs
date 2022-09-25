using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ImageCabinet.UIHelper
{
    public class BrushToValueAdjustedBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double valueAdjustment)) return DependencyProperty.UnsetValue;
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
                ColorToHSV(color, out double hue, out double saturation, out double brightness);
                brightness = Math.Max(0.0, Math.Min(1.0, brightness + valueAdjustment));
                var adjustedColor = ColorFromHSV(hue, saturation, brightness);
                adjustedColor.A = color.A;
                return new SolidColorBrush(adjustedColor);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        private static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            var drawingColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = drawingColor.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = System.Convert.ToInt32(Math.Floor(hue / 60.0)) % 6;
            double f = hue / 60.0 - Math.Floor(hue / 60.0);

            value = value * 255.0;
            var v = (byte)System.Convert.ToInt32(value);
            var p = (byte)System.Convert.ToInt32(value * (1 - saturation));
            var q = (byte)System.Convert.ToInt32(value * (1 - f * saturation));
            var t = (byte)System.Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
    }
}
