using System;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace ImageCabinet.UIHelper
{
    public class UIHelper
    {
        private static readonly DependencyProperty OffToggledContentProperty = DependencyProperty.RegisterAttached("OffToggledContent", typeof(object), typeof(UIHelper));
        public static void SetOffToggledContent(UIElement element, object value)
        {
            element.SetValue(OffToggledContentProperty, value);
        }
        public static object GetOffToggledContent(UIElement element)
        {
            return element.GetValue(OffToggledContentProperty);
        }

        public static readonly DependencyProperty ToggledContentProperty = DependencyProperty.RegisterAttached("ToggledContent", typeof(object), typeof(UIHelper), new PropertyMetadata()
        {
            PropertyChangedCallback = OnToggledContentChanged
        });
        public static void SetToggledContent(UIElement element, object value)
        {
            element.SetValue(ToggledContentProperty, value);
        }
        public static object GetToggledContent(UIElement element)
        {
            return element.GetValue(ToggledContentProperty);
        }

        private static void OnToggledContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ToggleButton toggleButton)) return;
            if (GetOffToggledContent(toggleButton) == null)
            {
                SetOffToggledContent(toggleButton, toggleButton.Content);
            }
            toggleButton.Checked -= ToggleButton_Checked;
            toggleButton.Unchecked -= ToggleButton_Unchecked;
            if (e.NewValue != null)
            {
                toggleButton.Checked += ToggleButton_Checked;
                toggleButton.Unchecked += ToggleButton_Unchecked;
            }
        }

        private static void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleButton toggleButton)) return;
            var offContent = GetOffToggledContent(toggleButton);
            if (offContent != null)
            {
                toggleButton.Content = offContent;
            }
        }

        private static void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleButton toggleButton)) return;
            var toggledContent = GetToggledContent(toggleButton);
            if (toggledContent != null)
            {
                toggleButton.Content = toggledContent;
            }
        }

        public static BitmapImage? GetDownscaledBitmapImage(string path, int? maxWidth, int? maxHeight)
        {
            try
            {
                if (File.Exists(path))
                {
                    int width = maxWidth.GetValueOrDefault();
                    int height = maxHeight.GetValueOrDefault();
                    using (var imageStream = File.OpenRead(path))
                    {
                        var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                        width = decoder.Frames[0].PixelWidth;
                        height = decoder.Frames[0].PixelHeight;
                    }

                    BitmapImage bitmapImage = new();
                    bitmapImage.BeginInit();
                    bitmapImage.UriCachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.Default);
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bitmapImage.UriSource = new Uri(path);
                    if (width > 0 && width >= height)
                    {
                        bitmapImage.DecodePixelWidth = maxWidth.HasValue ? Math.Min(width, maxWidth.GetValueOrDefault()) : width;
                    }
                    else if (height > 0)
                    {
                        bitmapImage.DecodePixelHeight = maxHeight.HasValue ? Math.Min(height, maxHeight.GetValueOrDefault()) : height;
                    }
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return bitmapImage;
                }
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}
