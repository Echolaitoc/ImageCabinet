using System.Windows;
using System.Windows.Controls;

namespace ImageCabinet.Settings
{
    public class SettingTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ThemeSettingTemplate { get; set; }
        public DataTemplate BoolSettingTemplate { get; set; }
        public DataTemplate IntSettingTemplate { get; set; }
        public DataTemplate DoubleSettingTemplate { get; set; }
        public DataTemplate StringSettingTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is SettingsItem setting)
            {
                if (setting.CustomSettingType == SettingsItem.THEME_SETTING_TYPE)
                {
                    return ThemeSettingTemplate;
                }
                else if (setting.TargetPropertyInfo.PropertyType == typeof(bool))
                {
                    return BoolSettingTemplate;
                }
                else if (setting.TargetPropertyInfo.PropertyType == typeof(int))
                {
                    return IntSettingTemplate;
                }
                else if (setting.TargetPropertyInfo.PropertyType == typeof(double))
                {
                    return DoubleSettingTemplate;
                }
                else if (setting.TargetPropertyInfo.PropertyType == typeof(string))
                {
                    return StringSettingTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
