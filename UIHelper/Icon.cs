using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageCabinet.UIHelper
{
    internal static class Icon
    {
        internal const int DEFAULT_SIZE = 24;

        public static ImageSource? GetImageSource(string key, int width = DEFAULT_SIZE, int height = DEFAULT_SIZE)
        {
            if (string.IsNullOrEmpty(key))
            {
                System.Diagnostics.Trace.WriteLine("IconExtension Error: Key empty!");
                return null;
            }
            var path = "icons." + key + ".xaml";

            var content = GetFromResources(path);
            if (!(content is FrameworkElement frameworkElement))
            {
                System.Diagnostics.Trace.WriteLine("IconExtension Error: Key '" + key + "' not found!");
                return null;
            }
            width = width <= 0 ? DEFAULT_SIZE : width;
            height = height <= 0 ? DEFAULT_SIZE : height;
            frameworkElement.Measure(new Size(width, height));
            frameworkElement.Arrange(new Rect(0, 0, width, height));
            if (frameworkElement.ActualWidth != width || frameworkElement.ActualHeight != height)
            {
                var transform = new ScaleTransform();
                if (width > frameworkElement.ActualWidth)
                {
                    transform.CenterX = frameworkElement.ActualWidth / 2;
                }
                if (width > frameworkElement.ActualHeight)
                {
                    transform.CenterY = frameworkElement.ActualHeight / 2;
                }
                transform.ScaleX = width / frameworkElement.ActualWidth;
                transform.ScaleY = height / frameworkElement.ActualHeight;
                frameworkElement.RenderTransform = transform;
                frameworkElement.Measure(new Size(width, height));
                frameworkElement.Arrange(new Rect(0, 0, width, height));
            }
            var bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(frameworkElement);

            return bmp;
        }

        public static Rectangle GetMaskedRectangle(string key, Brush fill, int width = DEFAULT_SIZE, int height = DEFAULT_SIZE)
        {
            width = width <= 0 ? DEFAULT_SIZE : width;
            height = height <= 0 ? DEFAULT_SIZE : height;
            var bmp = GetImageSource(key, width, height);
            if (bmp == null) return null;

            var rect = new Rectangle()
            {
                Width = width,
                Height = height,
                Fill = fill,
                OpacityMask = new VisualBrush()
                {
                    Visual = new Image()
                    {
                        Source = bmp,
                        Width = width,
                        Height = height,
                    }
                }
            };

            return rect;
        }

        private static object? GetFromResources(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream? stream = assembly.GetManifestResourceStream(assembly.GetName().Name + '.' + resourceName))
            {
                if (stream == null) return null;
                return XamlReader.Load(stream);
            }
        }
    }

    internal class IconExtension : MarkupExtension
    {
        private const string ERROR = "error";

        public string Key { get; set; }
        public int Width { get; set; } = Icon.DEFAULT_SIZE;
        public int Height { get; set; } = Icon.DEFAULT_SIZE;
        public Brush? Fill { get; set; } = null;

        public IconExtension(string key) : this(key, Icon.DEFAULT_SIZE) {}

        public IconExtension(string key, int size) : this(key, size, size) {}

        public IconExtension(string key, int width, int height)
        {
            Key = key;
            Width = width;
            Height = height;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var provideValueTarget = (IProvideValueTarget?)serviceProvider.GetService(typeof(IProvideValueTarget));
            var propertyType = (provideValueTarget?.TargetProperty as DependencyProperty)?.PropertyType;
            if (propertyType == typeof(ImageSource))
            {
                var bmp = Icon.GetImageSource(Key, Width, Height);
                return bmp == null ? DependencyProperty.UnsetValue : bmp;
            }
            var fill = GetFillBrush(provideValueTarget?.TargetObject as Control);
            return Icon.GetMaskedRectangle(Key, fill, Width, Height);
        }

        private Brush GetFillBrush(Control? target)
        {
            if (Fill == null)
            {
                Brush fill = Brushes.WhiteSmoke;
                if (target != null)
                {
                    fill = target.Foreground;
                }
                return fill;
            }
            else
            {
                return Fill;
            }
        }
    }
}
