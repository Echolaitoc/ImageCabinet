using System;

namespace ImageCabinet.Settings
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    internal class SaveableToXmlSettingAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    internal class GenerateSettingAttribute : SaveableToXmlSettingAttribute
    {
        public string? DisplayName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    internal class GenerateThemeSettingAttribute : GenerateSettingAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    internal class GenerateDirectorySettingAttribute : GenerateSettingAttribute
    {
    }
}
