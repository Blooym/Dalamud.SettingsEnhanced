using System;

namespace SettingsEnhanced.Game.Settings.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class UiSettingEnumAddonIdAttribute(uint nameAddonRow) : Attribute
    {
        public string UiName { get; } = Plugin.AddonSheet.GetRow(nameAddonRow).Text.ExtractText();
    }
}
