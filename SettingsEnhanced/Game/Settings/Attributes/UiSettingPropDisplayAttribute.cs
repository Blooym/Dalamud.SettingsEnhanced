using System;

namespace SettingsEnhanced.Game.Settings.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class UiSettingPropDisplayAttribute(uint nameAddonRow, uint groupAddonRow, uint headerAddonRow, bool interfaceIndented = false) : Attribute
    {
        public string UiName { get; } = nameAddonRow is 0 ? $"##{Guid.NewGuid()}" : $"{Plugin.AddonSheet.GetRow(nameAddonRow).Text.ExtractText()}##{Guid.NewGuid()}";
        public string UiGroup { get; } = groupAddonRow is 0 ? string.Empty : Plugin.AddonSheet.GetRow(groupAddonRow).Text.ExtractText();
        public string UiHeader { get; } = headerAddonRow is 0 ? string.Empty : Plugin.AddonSheet.GetRow(headerAddonRow).Text.ExtractText();
        public bool UiIndented { get; } = interfaceIndented;
    }
}
