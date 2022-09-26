using System;

namespace ImageCabinet.Settings
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    internal class GenerateSettingAttribute : Attribute
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
