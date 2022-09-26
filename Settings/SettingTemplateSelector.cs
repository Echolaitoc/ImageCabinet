using System.Windows;
using System.Windows.Controls;

namespace ImageCabinet.Settings
{
    public class SettingTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? DirectorySettingTemplate { get; set; }
        public DataTemplate? ThemeSettingTemplate { get; set; }
        public DataTemplate? BoolSettingTemplate { get; set; }
        public DataTemplate? IntSettingTemplate { get; set; }
        public DataTemplate? DoubleSettingTemplate { get; set; }
        public DataTemplate? StringSettingTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is SettingsItem setting)
            {
                if (setting.CustomSetting == SettingsItem.CustomSettingType.DirectorySelection)
                {
                    return GetDataTemplateIfExists(DirectorySettingTemplate, item, container);
                }
                else if (setting.CustomSetting == SettingsItem.CustomSettingType.Theme)
                {
                    return GetDataTemplateIfExists(ThemeSettingTemplate, item, container);
                }
                else if (setting.TargetPropertyInfo.PropertyType == typeof(bool))
                {
                    return GetDataTemplateIfExists(BoolSettingTemplate, item, container);
                }
                else if (setting.TargetPropertyInfo.PropertyType == typeof(int))
                {
                    return GetDataTemplateIfExists(IntSettingTemplate, item, container);
                }
                else if (setting.TargetPropertyInfo.PropertyType == typeof(double))
                {
                    return GetDataTemplateIfExists(DoubleSettingTemplate, item, container);
                }
                else if (setting.TargetPropertyInfo.PropertyType == typeof(string))
                {
                    return GetDataTemplateIfExists(StringSettingTemplate, item, container);
                }
            }
            return base.SelectTemplate(item, container);
        }

        private DataTemplate GetDataTemplateIfExists(DataTemplate? dataTemplate, object item, DependencyObject container)
        {
            if (dataTemplate != null)
            {
                return dataTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
