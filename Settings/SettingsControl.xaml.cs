using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace ImageCabinet.Settings
{
    public partial class SettingsControl : UserControl
    {
        public ObservableCollection<SettingsItem> SettingsList { get; set; } = new();

        public SettingsControl()
        {
            InitializeSettings();
            InitializeComponent();
        }

        private void InitializeSettings()
        {
            var configProperties = Config.Current.GetType().GetProperties();
            foreach (var property in configProperties)
            {
                var attributes = property.GetCustomAttributes(typeof(GenerateSettingAttribute), true);
                if (attributes != null && attributes.Length > 0)
                {
                    var setting = new SettingsItem(property);
                    if (attributes.Any(a => a is GenerateThemeSettingAttribute))
                    {
                        setting.CustomSettingType = SettingsItem.THEME_SETTING_TYPE;
                    }
                    SettingsList.Add(setting);
                }
            }
        }
    }
}
