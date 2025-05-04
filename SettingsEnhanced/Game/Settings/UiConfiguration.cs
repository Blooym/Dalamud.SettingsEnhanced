using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Game.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SettingsEnhanced.Game.Settings.Attributes;
using SettingsEnhanced.Game.Settings.Enums;
using SettingsEnhanced.Game.Settings.Interfaces;
using SettingsEnhanced.Game.Settings.Util;

namespace SettingsEnhanced.Game.Settings
{

    [JsonConverter(typeof(JsonConverter))]
    internal sealed partial class UiConfiguration : IGameConfiguration<UiConfiguration>, ICloneable
    {
#pragma warning disable CS8618

        [UiConfigurationItem(UiControlOption.MoveMode)]
        [UiSettingPropDisplay(7550, 7510, 7550)]
        public MovementType MoveMode { get; private set; }

        [UiConfigurationItem(UiConfigOption.BattleEffectSelf)]
        [UiSettingPropDisplay(7615, 7510, 7618)]
        public BattleEffects BattleEffectSelf { get; private set; }

        [UiConfigurationItem(UiConfigOption.BattleEffectParty)]
        [UiSettingPropDisplay(7616, 7510, 7618)]
        public BattleEffects BattleEffectParty { get; private set; }

        [UiConfigurationItem(UiConfigOption.BattleEffectOther)]
        [UiSettingPropDisplay(7617, 7510, 7618)]
        public BattleEffects BattleEffectOther { get; private set; }

        [UiConfigurationItem(UiConfigOption.BattleEffectPvPEnemyPc)]
        [UiSettingPropDisplay(7618, 7510, 7618)]
        public BattleEffects BattleEffectPvPOpponent { get; private set; }

        [UiConfigurationItem(UiConfigOption.IdleEmoteTime)]
        [UiSettingPropDisplay(8254, 4045, 7600)]
        [ConfigurationInputRange(0, 30)]
        public uint IdleAnimationDelay { get; private set; }

        [UiConfigurationItem(UiConfigOption.IdleEmoteRandomType)]
        [UiSettingPropDisplay(8257, 4045, 7600)]
        public bool IdleEmoteRandomType { get; private set; }

        [UiConfigurationItem(UiConfigOption.ShopConfirm)]
        [UiSettingPropDisplay(7611, 7517, 7610)]
        public bool ShopConfirmOnSell { get; private set; }

        [UiConfigurationItem(UiConfigOption.ShopConfirmMateria)]
        [UiSettingPropDisplay(7612, 7517, 7610, true)]
        public bool ShopConfirmMeldableItems { get; private set; }

        [UiConfigurationItem(UiConfigOption.ShopConfirmSpiritBondMax)]
        [UiSettingPropDisplay(7619, 7517, 7610, true)]
        public bool ShopConfirmSpiritbondMax { get; private set; }

        [UiConfigurationItem(UiConfigOption.ShopConfirmExRare)]
        [UiSettingPropDisplay(7612, 7517, 7610, true)]
        public bool ShopConfirmRareItem { get; private set; }

        [UiConfigurationItem(UiConfigOption.EmoteTextType)]
        [UiSettingPropDisplay(8120, 7514, 1911, true)]
        public bool EmoteDisplayLogMessage { get; private set; }

        [UiConfigurationItem(UiControlOption.FlyTextDisp)]
        [UiSettingPropDisplay(7679, 7642, 7650)]
        public bool HudDisplayFlyingText { get; private set; }

        [UiConfigurationItem(UiConfigOption.FlyTextDispSize)]
        [UiSettingPropDisplay(7697, 7642, 7650)]
        public FlyTextSize HudFlyingTextSize { get; private set; }

        [UiConfigurationItem(UiControlOption.PopUpTextDisp)]
        [UiSettingPropDisplay(10268, 7642, 7650)]
        public bool HudDisplayPopupText { get; private set; }

        [UiConfigurationItem(UiConfigOption.FlyTextDispSize)]
        [UiSettingPropDisplay(10269, 7642, 7650)]
        public PopupTextSize HudPopupTextSize { get; private set; }

        [UiConfigurationItem(UiControlOption.CharaParamDisp)]
        [UiSettingPropDisplay(7651, 7642, 7650)]
        public bool HudDisplayParameterBars { get; private set; }

        [UiConfigurationItem(UiControlOption.ExpDisp)]
        [UiSettingPropDisplay(7652, 7642, 7650)]
        public bool HudDisplayExpBar { get; private set; }

        [UiConfigurationItem(UiControlOption.InventryStatusDisp)]
        [UiSettingPropDisplay(7654, 7642, 7650)]
        public bool HudDisplayInventoryGrid { get; private set; }

        [UiConfigurationItem(UiControlOption.DutyListDisp)]
        [UiSettingPropDisplay(7655, 7642, 7650)]
        public bool HudDisplayDutyList { get; private set; }

        [UiConfigurationItem(UiControlOption.DutyListNumDisp)]
        [UiSettingPropDisplay(8075, 7642, 7650, true)]
        [ConfigurationInputRange(1, 10)]
        public uint HudDisplayDutyCount { get; private set; }

        [UiConfigurationItem(UiControlOption.InInstanceContentDutyListDisp)]
        [UiSettingPropDisplay(10225, 7642, 7650, true)]
        public bool HudDisplayDutyListInstanceDuty { get; private set; }

        [UiConfigurationItem(UiControlOption.InPublicContentDutyListDisp)]
        [UiSettingPropDisplay(10229, 7642, 7650, true)]
        public bool HudDisplayDutyListNonInstanceDuty { get; private set; }

        [UiConfigurationItem(UiControlOption.ContentsInfoJoiningRequestDisp)]
        [UiSettingPropDisplay(7708, 7642, 7650, true)]
        public bool HudDisplayRegisteredDutiesTimers { get; private set; }

        [UiConfigurationItem(UiControlOption.ContentsInfoJoiningRequestSituationDisp)]
        [UiSettingPropDisplay(7709, 7642, 7650, true)]
        public bool HudDisplayRegisteredDutyDetailTimers { get; private set; }

        [UiConfigurationItem(UiControlOption.NaviMapDisp)]
        [UiSettingPropDisplay(7656, 7642, 7650)]
        public bool HudDisplayMinimap { get; private set; }

        [UiConfigurationItem(UiControlOption.GilStatusDisp)]
        [UiSettingPropDisplay(7657, 7642, 7650)]
        public bool HudDisplayGil { get; private set; }

        [UiConfigurationItem(UiControlOption.InfoSettingDisp)]
        [UiSettingPropDisplay(7668, 7642, 7650)]
        public bool HudDisplayServerInfo { get; private set; }

        [UiConfigurationItem(UiControlOption.InfoSettingDispType)]
        [UiSettingPropDisplay(7699, 7642, 7650)]
        public ServerClockType HudDisplayServerClockType { get; private set; }

        [UiConfigurationItem(UiConfigOption.TimeEorzea)]
        [UiSettingPropDisplay(8065, 7642, 7650, true)]
        public bool HudDisplayServerInfoEorzeaTime { get; set; }

        [UiConfigurationItem(UiConfigOption.TimeLocal)]
        [UiSettingPropDisplay(8066, 7642, 7650, true)]
        public bool HudDisplayServerInfoLocalTime { get; set; }

        [UiConfigurationItem(UiConfigOption.TimeServer)]
        [UiSettingPropDisplay(8067, 7642, 7650, true)]
        public bool HudDisplayServerInfoServerTIme { get; set; }

        [UiConfigurationItem(UiConfigOption.InfoSettingDispWorldNameType)]
        [UiSettingPropDisplay(7734, 7642, 7650, true)]
        public bool HudDisplayServerInfoCurrentWorld { get; set; }

        [UiConfigurationItem(UiControlOption.LimitBreakGaugeDisp)]
        [UiSettingPropDisplay(7831, 7642, 7650)]
        public bool HudDisplayLimitGauge { get; private set; }

        [UiConfigurationItem(UiControlOption.ScenarioTreeCompleteDisp)]
        [UiSettingPropDisplay(10277, 7642, 7650)]
        public bool HudDisplayScenarioInfo { get; private set; }

        [UiConfigurationItem(UiControlOption.ScenarioTreeCompleteDisp)]
        [UiSettingPropDisplay(10278, 7642, 7650, true)]
        public bool HudHideScenarioComplete { get; private set; }

        [UiConfigurationItem(UiConfigOption.BattleTalkShowFace)]
        [UiSettingPropDisplay(10321, 7642, 7650)]
        public bool HudDisplayBattleTextPortraits { get; private set; }

        [UiConfigurationItem(UiControlOption.PartyListDisp)]
        [UiSettingPropDisplay(7671, 7642, 7670)]
        public bool HudDisplayPartyList { get; private set; }

        [UiConfigurationItem(UiControlOption.PartyListSoloOff)]
        [UiSettingPropDisplay(7673, 7642, 7670, true)]
        public bool HudHidePartyListSolo { get; private set; }

#pragma warning restore CS8618


        /// <summary>
        ///     Properties that will be kept across serialisation and deserialisation.
        /// </summary>
        private readonly HashSet<string> persistedProperties = [];

        /// <summary>
        ///     Properties that have been modified since the last time this configuration was applied.
        /// </summary>
        private readonly HashSet<string> modifiedProperties = [];

        /// <inheritdoc />
        public UiConfiguration PersistAllProperties()
        {
            foreach (var prop in this.GetType().GetProperties(Plugin.ConfigReflectionBindingFlags))
            {
                this.persistedProperties.Add(prop.Name);
            }
            return this;
        }

        /// <inheritdoc />
        public UiConfiguration DepersistAllProperties()
        {
            this.persistedProperties.Clear();
            return this;
        }

        /// <inheritdoc />
        public bool AnyPersistedProperties() => this.persistedProperties.Count != 0;

        /// <inheritdoc />
        public bool IsPropertyPersisted(PropertyInfo prop) => this.persistedProperties.Contains(prop.Name);

        /// <inheritdoc />
        public bool PersistProperty(PropertyInfo prop) => this.persistedProperties.Add(prop.Name);

        /// <inheritdoc />
        public bool DepersistProperty(PropertyInfo prop) => this.persistedProperties.Remove(prop.Name);

        /// <inheritdoc />
        public void SetPropertyValue<T>(PropertyInfo prop, T value)
        {
            if (!prop.CanWrite)
            {
                throw new InvalidOperationException($"Property {prop} is read-only.");
            }
            prop.SetValue(this, value);
            this.modifiedProperties.Add(prop.Name);
        }

        /// <inheritdoc />
        public object Clone()
        {
            // Jank to get deep clone.
            var serializedObject = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<UiConfiguration>(serializedObject)!;
        }

        /// <summary>
        ///     Read all settings from the game into a new instance of this class.
        /// </summary>
        public static UiConfiguration FromGame()
        {
            var uiConfiguration = new UiConfiguration();
            foreach (var prop in typeof(UiConfiguration).GetProperties(Plugin.ConfigReflectionBindingFlags))
            {
                var configOptionAttribute = prop.GetCustomAttribute<UiConfigurationItemAttribute>();
                if (configOptionAttribute is not null)
                {
                    if (configOptionAttribute.ConfigOption is not null && GameConfigUtil.TryGetGameConfigValue(configOptionAttribute.ConfigOption, prop.PropertyType, out var uiConfigValue))
                    {
                        prop.SetValue(uiConfiguration, uiConfigValue);
                    }
                    else if (configOptionAttribute.ControlOption is not null && GameConfigUtil.TryGetGameConfigValue(configOptionAttribute.ControlOption, prop.PropertyType, out var uiControlValue))
                    {
                        prop.SetValue(uiConfiguration, uiControlValue);
                    }
                }
            }
            return uiConfiguration;
        }

        /// <summary>
        ///     Apply all settings in this instance to the current game settings.
        /// </summary>
        /// <param name="onlyApplyModified">Only apply properties that have been modified since the last time this configuration was applied.</param>
        public void ApplyToGame(bool onlyApplyModified)
        {
            foreach (var prop in typeof(UiConfiguration)
                .GetProperties(Plugin.ConfigReflectionBindingFlags)
                .Where(p => !onlyApplyModified || this.modifiedProperties.Contains(p.Name)))
            {
                var configOptionAttribute = prop.GetCustomAttribute<UiConfigurationItemAttribute>();
                if (configOptionAttribute is not null)
                {
                    var value = prop.GetValue(this);
                    if (value is null)
                    {
                        continue;
                    }
                    Plugin.Log.Debug($"Applying {prop.Name}:{value} to game");
                    if (configOptionAttribute.ConfigOption.HasValue)
                    {
                        var configOption = configOptionAttribute.ConfigOption.Value;

                        if (prop.PropertyType.IsEnum)
                        {
                            var enumValue = Convert.ToUInt32(value);
                            Plugin.GameConfig.Set(configOption, enumValue);
                        }
                        else if (prop.PropertyType == typeof(uint))
                        {
                            Plugin.GameConfig.Set(configOption, (uint)value);
                        }
                        else if (prop.PropertyType == typeof(bool))
                        {
                            Plugin.GameConfig.Set(configOption, (bool)value);
                        }
                        else if (prop.PropertyType == typeof(string))
                        {
                            Plugin.GameConfig.Set(configOption, (string)value);
                        }
                    }
                    else if (configOptionAttribute.ControlOption.HasValue)
                    {
                        var controlOption = configOptionAttribute.ControlOption.Value;

                        if (prop.PropertyType.IsEnum)
                        {
                            var enumValue = Convert.ToUInt32(value);
                            Plugin.GameConfig.Set(controlOption, enumValue);
                        }
                        else if (prop.PropertyType == typeof(uint))
                        {
                            Plugin.GameConfig.Set(controlOption, (uint)value);
                        }
                        else if (prop.PropertyType == typeof(bool))
                        {
                            Plugin.GameConfig.Set(controlOption, (bool)value);
                        }
                        else if (prop.PropertyType == typeof(string))
                        {
                            Plugin.GameConfig.Set(controlOption, (string)value);
                        }
                    }
                }
                this.modifiedProperties.Remove(prop.Name);
            }
        }

        private sealed class JsonConverter : JsonConverter<UiConfiguration>
        {
            public override void WriteJson(JsonWriter writer, UiConfiguration? value, JsonSerializer serializer)
            {
                if (value is null)
                {
                    throw new InvalidOperationException("Attempt to write null value");
                }
                var onlyPersisted = new JObject();
                foreach (var prop in typeof(UiConfiguration).GetProperties(Plugin.ConfigReflectionBindingFlags))
                {
                    var currentValue = prop.GetValue(value);
                    if (value.IsPropertyPersisted(prop) && currentValue is not null)
                    {
                        Plugin.Log.Verbose($"Serialising persisted property {prop.Name} ({prop.MemberType}) on UiConfiguration");
                        onlyPersisted.Add(prop.Name, JToken.FromObject(currentValue));
                    }
                }
                onlyPersisted.WriteTo(writer);
            }

            public override UiConfiguration ReadJson(JsonReader reader, Type objectType, UiConfiguration? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                existingValue = FromGame();
                var jObject = JObject.Load(reader);
                foreach (var jproperty in jObject.Properties())
                {
                    var property = objectType.GetProperty(jproperty.Name);
                    if (property is not null)
                    {
                        var value = jproperty.Value.ToObject(property.PropertyType, serializer);
                        existingValue.persistedProperties.Add(property.Name);
                        property.SetValue(existingValue, value);
                        Plugin.Log.Verbose($"Deserializing persisted property {property.Name} ({property.MemberType}) on UiConfiguration");
                    }
                }
                return existingValue;
            }
        }
    }
}
