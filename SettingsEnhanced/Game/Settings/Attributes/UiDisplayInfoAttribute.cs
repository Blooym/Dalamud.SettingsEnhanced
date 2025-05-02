using System;
using SettingsEnhanced.Resources.Localization;

namespace SettingsEnhanced.Game.Settings.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UiDisplayInfoAttribute(string interfaceName, string interfaceGroupName, string interfaceHeaderName = "", bool interfaceIndented = false) : Attribute
    {
        public string InterfaceName { get; } = Strings.ResourceManager.GetString(interfaceName) ?? interfaceName;
        public string InterfaceGroupName { get; } = Strings.ResourceManager.GetString(interfaceGroupName) ?? interfaceGroupName;
        public string InterfaceHeaderName { get; } = Strings.ResourceManager.GetString(interfaceHeaderName) ?? interfaceHeaderName;
        public bool InterfaceIndented { get; } = interfaceIndented;
    }
}
