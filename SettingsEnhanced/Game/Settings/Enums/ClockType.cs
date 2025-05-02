using SettingsEnhanced.Game.Settings.Attributes;

namespace SettingsEnhanced.Game.Settings.Enums
{
    public enum ServerClockType
    {
        [UiSettingEnumAddonId(10316)]
        DefaultToLanguage = 0,
        [UiSettingEnumAddonId(10317)]
        TwentyFourHour = 1,
        [UiSettingEnumAddonId(10318)]
        TwelveHour = 2,
    }
}
