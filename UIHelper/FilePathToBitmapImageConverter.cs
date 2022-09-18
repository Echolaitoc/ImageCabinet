using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace ImageCabinet.UIHelper
{
    internal class FilePathToBitmapImageConverter : IValueConverter
    {
        public int? DecodePixelWidth { get; set; } = null;
        public int? DecodePixelHeight { get; set; } = null;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string uri && File.Exists(uri))
            {
                var bmp = UIHelper.GetDownscaledBitmapImage(uri, null, null);
                if (bmp != null)
                {
                    return bmp;
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
