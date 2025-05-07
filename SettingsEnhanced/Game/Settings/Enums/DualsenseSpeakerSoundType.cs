using SettingsEnhanced.Game.Settings.Attributes;

namespace SettingsEnhanced.Game.Settings.Enums
{
    public enum DualSenseSpeakerSoundType
    {
        [UiSettingEnumAddonId(8220)]
        AllSounds = 0,
        [UiSettingEnumAddonId(8221)]
        Notices = 1,
        [UiSettingEnumAddonId(8222)]
        None = 2
    }
}
