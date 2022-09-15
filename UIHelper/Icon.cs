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
    internal class IconExtension : MarkupExtension
    {
        private const string ERROR = "error";

        public string Key { get; set; }
        public int Width { get; set; } = 24;
        public int Height { get; set; } = 24;
        public Brush? Fill { get; set; } = null;

        public IconExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Key))
            {
                System.Diagnostics.Trace.WriteLine("IconExtension Error: Key empty!");
                return ERROR;
            }
            var path = "icons." + Key + ".xaml";
            
            var content = GetFromResources(path);
            if (!(content is FrameworkElement frameworkElement))
            {
                System.Diagnostics.Trace.WriteLine("IconExtension Error: Key '" + Key + "' not found!");
                return ERROR;
            }
            frameworkElement.Measure(new Size(Width, Height));
            frameworkElement.Arrange(new Rect(0, 0, Width, Height));
            if (frameworkElement.ActualWidth != Width || frameworkElement.ActualHeight != Height)
            {
                var transform = new ScaleTransform();
                if (Width > frameworkElement.ActualWidth)
                {
                    transform.CenterX = frameworkElement.ActualWidth / 2;
                }
                if (Height > frameworkElement.ActualHeight)
                {
                    transform.CenterY = frameworkElement.ActualHeight / 2;
                }
                transform.ScaleX = Width / frameworkElement.ActualWidth;
                transform.ScaleY = Height / frameworkElement.ActualHeight;
                frameworkElement.RenderTransform = transform;
                frameworkElement.Measure(new Size(Width, Height));
                frameworkElement.Arrange(new Rect(0, 0, Width, Height));
            }
            var bmp = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(frameworkElement);

            var provideValueTarget = (IProvideValueTarget?)serviceProvider.GetService(typeof(IProvideValueTarget));
            var propertyType = (provideValueTarget?.TargetProperty as DependencyProperty)?.PropertyType;
            if (propertyType == typeof(ImageSource))
            {
                return bmp;
            }
            var rect = new Rectangle()
            {
                Width = Width,
                Height = Height,
                Fill = GetFillBrush(provideValueTarget?.TargetObject as Control),
                OpacityMask = new VisualBrush()
                {
                    Visual = new Image()
                    {
                        Source = bmp,
                        Width = Width,
                        Height = Height,
                    }
                }
            };

            return rect;
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

        internal static object? GetFromResources(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream? stream = assembly.GetManifestResourceStream(assembly.GetName().Name + '.' + resourceName))
            {
                if (stream == null) return null;
                return XamlReader.Load(stream);
            }
        }
    }
}
