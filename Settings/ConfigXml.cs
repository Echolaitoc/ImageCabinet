using System;
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

        public static void WriteSettingsToXml()
        {
            var changedProperties = Config.Current.GetChangedSaveableProperties();
            if (!Config.Current.AnyValueChanged && !changedProperties.Any()) return;
            XmlDocument doc = new XmlDocument();
            var root = doc.CreateNode(XmlNodeType.Element, nameof(Config), "");
            foreach (var property in changedProperties)
            {
                XmlNode node = doc.CreateNode(XmlNodeType.Element, "Setting", "");
                var propertyAttribute = doc.CreateAttribute(ATTRIBUTE_PROPERTY);
                propertyAttribute.Value = property.Name;
                node.Attributes?.Append(propertyAttribute);
                var valueAttribute = doc.CreateAttribute(ATTRIBUTE_VALUE);
                var valueString = property.GetValue(Config.Current)?.ToString();
                if (property.PropertyType == typeof(double) && UIHelper.UIHelper.TryParseDouble(valueString, out double doubleValue))
                {
                    valueAttribute.Value = doubleValue.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    valueAttribute.Value = valueString;
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
