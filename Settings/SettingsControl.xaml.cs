using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageCabinet.Settings
{
    public partial class SettingsControl : UserControl
    {
        public ICommand BrowseForDirectoryCommand { get; set; } = new RoutedUICommand();
        public ObservableCollection<SettingsItem> SettingsList { get; set; } = new();

        public SettingsControl()
        {
            InitializeSettings();
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(BrowseForDirectoryCommand, BrowseForDirectory));
        }

        private void InitializeSettings()
        {
            var configProperties = Config.Current.GetType().GetProperties();
            foreach (var property in configProperties)
            {
                var attributes = property.GetCustomAttributes(typeof(GenerateSettingAttribute), true);
                if (attributes != null && attributes.Length > 0)
                {
                    string displayName = property.Name;
                    var generateSetting = attributes.FirstOrDefault(a => a is GenerateSettingAttribute generateSetting && generateSetting.DisplayName != null) as GenerateSettingAttribute;
                    if (generateSetting != null && generateSetting.DisplayName != null)
                    {
                        displayName = generateSetting.DisplayName;
                    }
                    SettingsItem.CustomSettingType customSetting = SettingsItem.CustomSettingType.Default;
                    if (attributes.Any(a => a is GenerateDirectorySettingAttribute))
                    {
                        customSetting = SettingsItem.CustomSettingType.DirectorySelection;
                    }
                    if (attributes.Any(a => a is GenerateThemeSettingAttribute))
                    {
                        customSetting = SettingsItem.CustomSettingType.Theme;
                    }
                    var setting = new SettingsItem(property, customSetting, displayName);
                    SettingsList.Add(setting);
                }
            }
        }

        private void IntSetting_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!IsIntTextAllowed(e.Text))
            {
                e.Handled = true;
            }
        }

        private void IntSetting_Pasting(object sender, System.Windows.DataObjectPastingEventArgs e)
        {
            HandlePasting(ref e, IsIntTextAllowed);
        }

        private bool IsIntTextAllowed(string text)
        {
            return int.TryParse(text, out int _);
        }

        private void DoubleSetting_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!IsDoubleTextAllowed(e.Text))
            {
                e.Handled = true;
            }
        }

        private void DoubleSetting_Pasting(object sender, System.Windows.DataObjectPastingEventArgs e)
        {
            HandlePasting(ref e, IsDoubleTextAllowed);
        }

        private bool IsDoubleTextAllowed(string text)
        {
            if (!string.IsNullOrEmpty(text) && (text.EndsWith('.') || text.EndsWith(',')))
            {
                text += "0";
            }
            return double.TryParse(text, out double _);
        }

        private void HandlePasting(ref System.Windows.DataObjectPastingEventArgs e, Func<string, bool> IsTextAllowed)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void BrowseForDirectory(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.Parameter is SettingsItem setting)) return;

            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new();
            if (setting.Value is string path && System.IO.Directory.Exists(path))
            {
                dialog.SelectedPath = path + System.IO.Path.DirectorySeparatorChar;
            }
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                setting.Value = dialog.SelectedPath;
            }
        }
    }
}
