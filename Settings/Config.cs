﻿using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace ImageCabinet.Settings
{
    public class Config
    {
        #region Config Initialization
        private static Config? _current;
        public static Config Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new();
                }
                return _current;
            }
        }

        private Config() {}

        public void Initialize(StartupEventArgs args)
        {
            for (int i = 0; i < args.Args.Length; ++i)
            {
                var arg = args.Args[i];
                
                var property = GetPropertyFromArg(arg);
                var value = GetValueFromArg(arg);
                if (string.IsNullOrEmpty(property) || string.IsNullOrEmpty(value))
                {
                    System.Diagnostics.Trace.WriteLine("[ImageCabinet.Config.Initialize] can't convert argument to property/value couple: " + arg);
                }
                else if (GetType().GetProperty(property) is PropertyInfo propInfo)
                {
                    try
                    {
                        var valueObj = TypeDescriptor.GetConverter(propInfo.PropertyType).ConvertFromString(null, System.Globalization.CultureInfo.InvariantCulture, value);
                        if (valueObj != null)
                        {
                            propInfo.SetValue(this, valueObj, null);
                        }
                    }
                    catch
                    {
                        System.Diagnostics.Trace.WriteLine("[ImageCabinet.Config.Initialize] can't set " + property + " to value " + value);
                    }
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("[ImageCabinet.Config.Initialize] unknown property " + property);
                }
            }
        }

        private string GetPropertyFromArg(string arg)
        {
            if (string.IsNullOrEmpty(arg)) return string.Empty;

            var startIndex = arg.IndexOf(@"/") + 1;
            var length = arg.IndexOf(@"=") - startIndex;
            if (startIndex <= 0 || length < 0) return string.Empty;
            return arg.Substring(startIndex, length);
        }

        private string GetValueFromArg(string arg)
        {
            if (string.IsNullOrEmpty(arg)) return string.Empty;

            var startIndex = arg.IndexOf(@"=") + 1;
            if (startIndex <= 0) return string.Empty;
            return arg.Substring(startIndex);
        }
        #endregion Config Initialization

        [GenerateDirectorySetting(DisplayName = "Startup directory")]
        public string StartupDirectory { get; private set; } = string.Empty;

        [GenerateThemeSetting]
        public string Theme { get; private set; } = "DefaultDark";

        [GenerateSetting(DisplayName = "Foo")]
        public bool TestBool { get; private set; } = true;

        [GenerateSetting]
        public double TestDouble { get; private set; } = 1.5;

        [GenerateSetting]
        public int TestInt { get; private set; } = 1;
    }
}
