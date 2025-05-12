using System;
using System.Collections.Generic;
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

        [UiConfigurationItem(UiConfigOption.NamePlateDispTypeSelf)]
        [UiSettingPropDisplay(7703, 7512, 7700)]
        public DisplayNameSettings NamePlateDisplayTypeSelf { get; private set; }

        [UiConfigurationItem(UiConfigOption.NamePlateDispTypeParty)]
        [UiSettingPropDisplay(7703, 7512, 7710)]
        public DisplayNameSettings NamePlateDisplayTypeParty { get; private set; }

        [UiConfigurationItem(UiConfigOption.NamePlateDispTypeOther)]
        [UiSettingPropDisplay(7703, 7512, 7712)]
        public DisplayNameSettings NamePlateDisplayTypeOther { get; private set; }

        [UiConfigurationItem(UiConfigOption.NamePlateDispTypeFriend)]
        [UiSettingPropDisplay(7703, 7512, 7719)]
        public DisplayNameSettings NamePlateDisplayTypeFriend { get; private set; }
#pragma warning restore CS8618


        /// <summary>
        ///     Properties that will be kept across serialisation and deserialisation.
        /// </summary>
        private readonly HashSet<string> persistedProperties = [];

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
        public void ApplyToGame()
        {
            foreach (var prop in typeof(UiConfiguration).GetProperties(Plugin.ConfigReflectionBindingFlags))
            {
                var configOptionAttribute = prop.GetCustomAttribute<UiConfigurationItemAttribute>();
                if (configOptionAttribute is null)
                    continue;
                // If a property is not persisted fetch and update its value with an up-to-date value from the game
                // incase the value we got during serialisation has gone stale.
                if (!this.IsPropertyPersisted(prop))
                {
                    if (GameConfigUtil.TryGetGameConfigValue(configOptionAttribute.ConfigOption is not null ? configOptionAttribute.ConfigOption : configOptionAttribute.ControlOption!, prop.PropertyType, out var refreshedValue))
                    {
                        prop.SetValue(this, refreshedValue);
                        Plugin.Log.Verbose($"Refreshed {prop.Name} with value {refreshedValue} as it was not a persisted value.");
                    }
                }
                if (prop.GetValue(this) is object value)
                {
                    GameConfigUtil.SetGameConfigValue(
                        configOptionAttribute.ConfigOption is not null ? configOptionAttribute.ConfigOption : configOptionAttribute.ControlOption!,
                        prop.PropertyType,
                        value
                    );
                    Plugin.Log.Debug($"Applied {prop.Name}:{value} to game");
                }
            }
        }

        private sealed class JsonConverter : JsonConverter<UiConfiguration>
        {
            /// <inheritdoc />
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
                        Plugin.Log.Verbose($"Serialising persisted property {prop.Name}:{currentValue} ({prop.PropertyType}) on {typeof(UiConfiguration)}");
                        onlyPersisted.Add(prop.Name, JToken.FromObject(currentValue));
                    }
                }
                onlyPersisted.WriteTo(writer);
            }

            /// <inheritdoc />
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
                        Plugin.Log.Verbose($"Deserializing persisted property {property.Name}:{value} ({property.PropertyType}) on {typeof(UiConfiguration)}");
                    }
                }
                return existingValue;
            }
        }
    }
}
