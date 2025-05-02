using System;
using System.Diagnostics.CodeAnalysis;
using Dalamud.Game.Config;

namespace SettingsEnhanced.Game.Settings.Util
{
    internal static class GameConfigUtil
    {
        public static bool TryGetGameConfigValue(Enum configOption, Type valueType, [MaybeNullWhen(false)] out object value)
        {
            value = null;
            dynamic gameConfigOption = configOption switch
            {
                SystemConfigOption sysOption => sysOption,
                UiConfigOption uiOption => uiOption,
                UiControlOption uiControlOption => uiControlOption,
                _ => throw new InvalidOperationException("Unknown type")
            };
            if (valueType.IsEnum)
            {
                if (Plugin.GameConfig.TryGet(gameConfigOption, out uint enumValue))
                {
                    value = Enum.ToObject(valueType, enumValue);
                }
            }
            else if (valueType == typeof(uint))
            {
                if (Plugin.GameConfig.TryGet(gameConfigOption, out uint intValue))
                {
                    value = intValue;
                }
            }
            else if (valueType == typeof(bool))
            {
                if (Plugin.GameConfig.TryGet(gameConfigOption, out bool boolValue))
                {
                    value = boolValue;
                }
            }
            else if (valueType == typeof(string))
            {
                if (Plugin.GameConfig.TryGet(gameConfigOption, out string stringValue))
                {
                    value = stringValue;
                }
            }
            Plugin.Log.Verbose($"TryGetGameConfigValue - ConfigOption: {configOption} ValueType: {valueType} Value: {value}");
            return value is not null;
        }
    }
}
