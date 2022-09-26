using System;

namespace ImageCabinet.Settings
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    internal class GenerateSettingAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    internal class GenerateThemeSettingAttribute : GenerateSettingAttribute
    {
    }
}
