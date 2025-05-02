using System;
using Dalamud.Game.Config;

namespace SettingsEnhanced.Game.Settings.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class SystemConfigurationItemAttribute(SystemConfigOption configOption) : Attribute
    {
        public SystemConfigOption ConfigOption { get; } = configOption;
    }
}
