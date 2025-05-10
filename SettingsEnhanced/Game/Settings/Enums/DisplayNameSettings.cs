using SettingsEnhanced.Game.Settings.Attributes;

namespace SettingsEnhanced.Game.Settings.Enums
{
    public enum DisplayNameSettings
    {
        [UiSettingEnumAddonId(7930)]
        Always = 0,
        [UiSettingEnumAddonId(7931)]
        DuringBattle = 1,
        [UiSettingEnumAddonId(7932)]
        WhenTargeted = 2,
        [UiSettingEnumAddonId(7933)]
        Never = 3,
        [UiSettingEnumAddonId(7934)]
        OutOfBattle = 4,
    }
}
