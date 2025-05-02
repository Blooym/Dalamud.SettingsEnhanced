using System;
using Dalamud.Game.Config;

namespace SettingsEnhanced.Game.Settings.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class UiConfigurationItemAttribute : Attribute
    {
        public UiConfigurationItemAttribute(UiConfigOption configOption) => this.ConfigOption = configOption;
        public UiConfigurationItemAttribute(UiControlOption controlOption) => this.ControlOption = controlOption;
        public UiConfigOption? ConfigOption { get; }
        public UiControlOption? ControlOption { get; }
    }
}
