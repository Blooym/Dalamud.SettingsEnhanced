using SettingsEnhanced.Game.Settings.Attributes;

namespace SettingsEnhanced.Game.Settings.Enums
{
    public enum SoundEqualizerType
    {
        [UiSettingEnumAddonId(8738)]
        Standard = 0,
        [UiSettingEnumAddonId(8739)]
        BassBoost = 1,
        [UiSettingEnumAddonId(8740)]
        TrebleBoost = 2,
        [UiSettingEnumAddonId(8741)]
        VoiceBoost = 3,
        [UiSettingEnumAddonId(8742)]
        LogitechProG50mm = 4
    }
}
