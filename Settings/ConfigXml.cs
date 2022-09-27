using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace ImageCabinet.Settings
{
    internal static class ConfigXml
    {
        private static string CONFIG_XML_FILENAME = "config.xml";
        private static string CONFIG_XML_DIRECTORY = Path.Combine(Environment.ExpandEnvironmentVariables(@"%Appdata%"), nameof(ImageCabinet));

        private static string ATTRIBUTE_PROPERTY = "Property";
        private static string ATTRIBUTE_VALUE = "Value";

        private static string ConfigFile { get { return Path.Combine(CONFIG_XML_DIRECTORY, CONFIG_XML_FILENAME); } }

        private static string? CurrentDocumentContent { get; set; }

        public static void ReadXmlToConfig()
        {
            if (!File.Exists(ConfigFile)) return;
            XmlDocument doc = new XmlDocument();
            doc.Load(ConfigFile);
            if (doc.ChildNodes.Count <= 1 || !(doc.ChildNodes[1] is XmlNode root)) return;
            foreach (XmlNode settingsNode in root.ChildNodes)
            {
                if (settingsNode.Attributes == null) continue;
                string? property = null;
                string? value = null;
                bool foundValue = false;
                foreach (XmlAttribute attribute in settingsNode.Attributes)
                {
                    if (attribute.Name == ATTRIBUTE_PROPERTY)
                    {
                        property = attribute.Value;
                    }
                    else if (attribute.Name == ATTRIBUTE_VALUE)
                    {
                        value = attribute.Value;
                        foundValue = true;
                    }
                }
                if (property != null && foundValue)
                {
                    Config.Current.TrySetValue(property, value);
                }
            }
            CurrentDocumentContent = doc.OuterXml;
        }

        public static void WriteSettingsToXml(IEnumerable<SettingsItem> settings)
        {
            if (settings == null) return;
            var changedSettings = settings.Where(s => Config.Current.IsValueChanged(s.TargetPropertyInfo));
            if (!changedSettings.Any()) return;
            XmlDocument doc = new XmlDocument();
            var root = doc.CreateNode(XmlNodeType.Element, nameof(Config), "");
            foreach (var setting in changedSettings)
            {
                XmlNode node = doc.CreateNode(XmlNodeType.Element, "Setting", "");
                var propertyAttribute = doc.CreateAttribute(ATTRIBUTE_PROPERTY);
                propertyAttribute.Value = setting.TargetPropertyInfo.Name;
                node.Attributes?.Append(propertyAttribute);
                var valueAttribute = doc.CreateAttribute(ATTRIBUTE_VALUE);
                if (setting.TargetPropertyInfo.PropertyType == typeof(double) && UIHelper.UIHelper.TryParseDouble(setting.Value?.ToString(), out double doubleValue))
                {
                    valueAttribute.Value = doubleValue.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    valueAttribute.Value = setting.Value?.ToString();
                }
                node.Attributes?.Append(valueAttribute);
                root.AppendChild(node);
            }
            doc.AppendChild(root);
            XmlDeclaration xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
            xmldecl.Encoding = "UTF-8";
            xmldecl.Standalone = "yes";
            doc.InsertBefore(xmldecl, root);
            var docContent = doc.OuterXml;
            if (docContent != CurrentDocumentContent)
            {
                if (!Directory.Exists(CONFIG_XML_DIRECTORY))
                {
                    Directory.CreateDirectory(CONFIG_XML_DIRECTORY);
                }
                doc.Save(ConfigFile);
                CurrentDocumentContent = docContent;
            }
        }
    }
}
