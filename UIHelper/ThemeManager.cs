using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace ImageCabinet.UIHelper
{
    public static class ThemeManager
    {
        private const string THEME_DIR = "themes/";
        private const string DEFAULT_THEME = "DefaultDark.xaml";
        private const string THEME_COLORS_RESOURCE = "pack://application:,,,/ThemeColors.xaml";

        private static ResourceDictionary? ThemeResourceDictionary { get; set; }
        private static ResourceDictionary? ThemeColorsResourceDictionary { get; set; }

        public static void LoadTheme(string theme)
        {
            if (string.IsNullOrEmpty(theme))
            {
                theme = DEFAULT_THEME;
            }
            var themePath = GetThemePath(theme);
            if (string.IsNullOrEmpty(themePath) || !File.Exists(themePath)) return;
            ThemeResourceDictionary = ReplaceResourceDictionary(ThemeResourceDictionary, new Uri(themePath, UriKind.Absolute));
            ThemeColorsResourceDictionary = ReplaceResourceDictionary(ThemeColorsResourceDictionary, new Uri(THEME_COLORS_RESOURCE, UriKind.Absolute));

            var testCol = GetResource("DefaultForeground", System.Windows.Media.Brushes.Magenta);
            System.Diagnostics.Trace.Write(testCol);
        }

        private static string GetThemePath(string theme)
        {
            if (!theme.EndsWith(".xaml"))
            {
                theme += ".xaml";
            }
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location);
            if (string.IsNullOrEmpty(baseDir)) return string.Empty;

            var fullPath = Path.Combine(baseDir, THEME_DIR, theme);
            if (!File.Exists(fullPath))
            {
                fullPath = Path.Combine(baseDir, DEFAULT_THEME);
            }
            return fullPath;
        }

        public static string GetThemeDirectoryPath()
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location);
            if (string.IsNullOrEmpty(baseDir)) return string.Empty;
            return Path.Combine(baseDir, THEME_DIR);
        }

        private static ResourceDictionary ReplaceResourceDictionary(ResourceDictionary? resourceDictionary, Uri uri)
        {
            if (resourceDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
            }
            resourceDictionary = new ResourceDictionary() { Source = uri };
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            return resourceDictionary;
        }

        public static T? GetResource<T>(object key)
        {
            return GetResource(key, default(T));
        }

        public static T? GetResource<T>(object key, T? fallbackValue)
        {
            if (key != null && Application.Current.TryFindResource(key) is T resource)
            {
                return resource;
            }
            var type = typeof(T);
            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
            {
                Activator.CreateInstance(typeof(T));
            }
            return fallbackValue;
        }
    }
}
