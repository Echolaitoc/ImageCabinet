using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace ImageCabinet.Settings
{
    public class SettingsItem : DependencyObject
    {
        public enum CustomSettingType
        {
            Default,
            Theme,
            DirectorySelection,
        }

        private event EventHandler? ValueChanged;

        public CustomSettingType CustomSetting { get; private set; } = CustomSettingType.Default;
        public PropertyInfo TargetPropertyInfo { get; private set; }
        public string DisplayName { get; private set; }
        private bool UpdateConfigValue { get; set; } = true;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(SettingsItem), new FrameworkPropertyMetadata()
        {
            PropertyChangedCallback = OnValueChanged
        });
        public object? Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueListProperty = DependencyProperty.Register("ValueList", typeof(List<object>), typeof(SettingsItem), new FrameworkPropertyMetadata());
        public List<object>? ValueList
        {
            get { return (List<object>)GetValue(ValueListProperty); }
            set { SetValue(ValueListProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is SettingsItem setting) || !setting.UpdateConfigValue) return;

            if (Config.Current.TrySetValue(setting.TargetPropertyInfo, e.NewValue) && setting.ValueChanged != null)
            {
                setting.ValueChanged.Invoke(setting, new EventArgs());
            }
        }

        public SettingsItem(PropertyInfo targetPropertyInfo, CustomSettingType customSetting, string displayName)
        {
            TargetPropertyInfo = targetPropertyInfo;
            CustomSetting = customSetting;
            DisplayName = displayName;
            UpdateConfigValue = false;
            switch (CustomSetting)
            {
                case CustomSettingType.DirectorySelection:
                    var path = targetPropertyInfo.GetValue(Config.Current, null) as string;
                    if (path != null)
                    {
                        path = Environment.ExpandEnvironmentVariables(path);
                    }
                    Value = path;
                    break;
                case CustomSettingType.Theme:
                    GenerateThemeList();
                    break;
                default:
                    Value = targetPropertyInfo.GetValue(Config.Current, null);
                    break;
            }
            UpdateConfigValue = true;
        }

        private void GenerateThemeList()
        {
            var themeList = new List<object>();
            var themeDirectory = UIHelper.ThemeManager.GetThemeDirectoryPath();
            if (!string.IsNullOrEmpty(themeDirectory) && Directory.Exists(themeDirectory))
            {
                var files = Directory.EnumerateFiles(themeDirectory);
                foreach (var file in files)
                {
                    if (!string.IsNullOrEmpty(file) && file.EndsWith(".xaml") && File.Exists(file))
                    {
                        var themeName = Path.GetFileNameWithoutExtension(file);
                        if (!string.IsNullOrEmpty(themeName))
                        {
                            themeList.Add(themeName);
                        }
                    }
                }
            }
            ValueList = themeList;
            Value = ValueList.FirstOrDefault(theme => (string)theme == Config.Current?.Theme);
            ValueChanged -= Theme_ValueChanged;
            ValueChanged += Theme_ValueChanged;
        }

        private void Theme_ValueChanged(object? sender, EventArgs e)
        {
            if (!(sender is SettingsItem setting) || !(setting.Value is string theme)) return;
            UIHelper.ThemeManager.LoadTheme(theme);
        }
    }
}
