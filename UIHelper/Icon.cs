using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageCabinet.UIHelper
{
    internal static class Icon
    {
        internal const int DEFAULT_SIZE = 24;

        public static ImageSource? GetImageSource(string key, Color? fill, bool addShadow = false, int width = DEFAULT_SIZE, int height = DEFAULT_SIZE)
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
            if (fill != null)
            {
                frameworkElement.Effect = new RecolorEffect(fill.GetValueOrDefault());
            }
            Visual visual = frameworkElement;
            if (addShadow)
            {
                var grid = new Grid();
                grid.Children.Add(frameworkElement);
                grid.Effect = GetShadow();
                visual = grid;
            }
            var bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(visual);

            return bmp;
        }

        public static Rectangle? GetMaskedRectangle(string key, Brush? fill, bool addShadow = false, int width = DEFAULT_SIZE, int height = DEFAULT_SIZE)
        {
            width = width <= 0 ? DEFAULT_SIZE : width;
            height = height <= 0 ? DEFAULT_SIZE : height;
            var bmp = GetImageSource(key, null, false, width, height);
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
            if (addShadow)
            {
                rect.Effect = GetShadow();
            }

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

        private static DropShadowEffect GetShadow()
        {
            return new DropShadowEffect() { BlurRadius = 5, Direction = 270 };
        }
    }

    internal class IconExtension : MarkupExtension
    {
        public string Key { get; set; }
        public int Width { get; set; } = Icon.DEFAULT_SIZE;
        public int Height { get; set; } = Icon.DEFAULT_SIZE;
        public bool ForceProvideImageSource { get; set; } = false;
        public bool AddShadow { get; set; } = false;
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
            if (propertyType == typeof(ImageSource) || ForceProvideImageSource)
            {
                Color? fillColor = null;
                if (Fill != null && Fill is SolidColorBrush solidFill)
                {
                    fillColor = solidFill.Color;
                }
                var bmp = Icon.GetImageSource(Key, fillColor, AddShadow, Width, Height);
                return bmp == null ? DependencyProperty.UnsetValue : bmp;
            }
            var rect = Icon.GetMaskedRectangle(Key, Fill, AddShadow, Width, Height);
            if (rect != null && Fill == null)
            {
                var foregroundBinding = new Binding()
                {
                    Path = new PropertyPath(Control.ForegroundProperty),
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                };
                rect.SetBinding(Shape.FillProperty, foregroundBinding);
            }
            return rect == null ? DependencyProperty.UnsetValue : rect;
        }
    }
}
