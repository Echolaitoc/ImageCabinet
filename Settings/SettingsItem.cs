using System.Reflection;
using System.Windows;

namespace ImageCabinet.Settings
{
    public class SettingsItem : DependencyObject
    {
        public const string THEME_SETTING_TYPE = nameof(THEME_SETTING_TYPE);

        public string CustomSettingType { get; set; } = string.Empty;
        public PropertyInfo TargetPropertyInfo { get; set; }
        public string PropertyName { get { return TargetPropertyInfo.Name; } }

        public SettingsItem(PropertyInfo targetPropertyInfo)
        {
            TargetPropertyInfo = targetPropertyInfo;
        }
    }
}
