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
using SettingsEnhanced.Resources.Localization;

namespace SettingsEnhanced.Game.Settings
{

    [JsonConverter(typeof(JsonConverter))]
    internal sealed partial class UiConfiguration : IGameConfiguration<UiConfiguration>, ICloneable
    {
#pragma warning disable CS8618

        [UiConfigurationItem(UiControlOption.MoveMode)]
        [UiDisplayInfo(nameof(Strings.UI_ConfigOption_Name_MoveMode), nameof(Strings.UI_ConfigOption_GroupName_ControlSettings), nameof(Strings.UI_ConfigOptior_HeaderName_MovementSettings))]
        public MovementType MoveMode { get; private set; }

        [UiConfigurationItem(UiConfigOption.BattleEffectSelf)]
        [UiDisplayInfo("Own", "Character Settings", "Battle Effects Settings")]
        public BattleEffects BattleEffectSelf { get; private set; }

        [UiConfigurationItem(UiConfigOption.BattleEffectParty)]
        [UiDisplayInfo("Party", "Character Settings", "Battle Effects Settings")]
        public BattleEffects BattleEffectParty { get; private set; }

        [UiConfigurationItem(UiConfigOption.BattleEffectOther)]
        [UiDisplayInfo("Other (excl. PvP)", "Character Settings", "Battle Effects Settings")]
        public BattleEffects BattleEffectOther { get; private set; }

        [UiConfigurationItem(UiConfigOption.BattleEffectPvPEnemyPc)]
        [UiDisplayInfo("PvP Opponents", "Character Settings", "Battle Effects Settings")]
        public BattleEffects BattleEffectPvPOpponent { get; private set; }

        [UiConfigurationItem(UiConfigOption.IdleEmoteTime)]
        [UiDisplayInfo("Idle Animation Delay", "Character Settings", "Animation")]
        [ConfigurationInputRange(0, 30)]
        public uint IdleAnimationDelay { get; private set; }

        [UiConfigurationItem(UiConfigOption.IdleEmoteRandomType)]
        [UiDisplayInfo("Randomize idle animation", "Character Settings", "Animation")]
        public bool IdleEmoteRandomType { get; private set; }

        [UiConfigurationItem(UiConfigOption.ShopConfirm)]
        [UiDisplayInfo("Display confirmation when selling items", "Item Settings", "Shop Settings")]
        public bool ShopConfirmOnSell { get; private set; }

        [UiConfigurationItem(UiConfigOption.ShopConfirmMateria)]
        [UiDisplayInfo("Meldable Items", "Item Settings", "Shop Settings", true)]
        public bool ShopConfirmMeldableItems { get; private set; }

        [UiConfigurationItem(UiConfigOption.ShopConfirmSpiritBondMax)]
        [UiDisplayInfo("Spiritbound Items", "Item Settings", "Shop Settings", true)]
        public bool ShopConfirmSpiritbondMax { get; private set; }

        [UiConfigurationItem(UiConfigOption.ShopConfirmExRare)]
        [UiDisplayInfo("Unique/Untradeable Items", "Item Settings", "Shop Settings", true)]
        public bool ShopConfirmRareItem { get; private set; }

        [UiConfigurationItem(UiConfigOption.EmoteTextType)]
        [UiDisplayInfo("Display Emote Log Message", "Chat Settings", "Emotes")]
        public bool EmoteDisplayLogMessage { get; private set; }

        [UiConfigurationItem(UiControlOption.FlyTextDisp)]
        [UiDisplayInfo("Display flying text", "HUD Settings", "Text")]
        public bool HudDisplayFlyingText { get; private set; }

        [UiConfigurationItem(UiConfigOption.FlyTextDispSize)]
        [UiDisplayInfo("Flying Text Size", "HUD Settings", "Text", true)]
        public FlyTextSize HudFlyingTextSize { get; private set; }

        [UiConfigurationItem(UiControlOption.PopUpTextDisp)]
        [UiDisplayInfo("Display pop-up text", "HUD Settings", "Text")]
        public bool HudDisplayPopupText { get; private set; }

        [UiConfigurationItem(UiConfigOption.FlyTextDispSize)]
        [UiDisplayInfo("Pop-up Text Size", "HUD Settings", "Text", true)]
        public PopupTextSize HudPopupTextSize { get; private set; }

        [UiConfigurationItem(UiControlOption.CharaParamDisp)]
        [UiDisplayInfo("Display parameter bars", "HUD Settings")]
        public bool HudDisplayParameterBars { get; private set; }

        [UiConfigurationItem(UiControlOption.ExpDisp)]
        [UiDisplayInfo("Display EXP bar", "HUD Settings")]
        public bool HudDisplayExpBar { get; private set; }

        [UiConfigurationItem(UiControlOption.InventryStatusDisp)]
        [UiDisplayInfo("Display inventory grid", "HUD Settings")]
        public bool HudDisplayInventoryGrid { get; private set; }

        [UiConfigurationItem(UiControlOption.DutyListDisp)]
        [UiDisplayInfo("Display duty list", "HUD Settings", "Duty List")]
        public bool HudDisplayDutyList { get; private set; }

        [UiConfigurationItem(UiControlOption.DutyListNumDisp)]
        [UiDisplayInfo("Number of Duties Displayed", "HUD Settings", "Duty List", true)]
        [ConfigurationInputRange(1, 10)]
        public uint HudDisplayDutyCount { get; private set; }

        [UiConfigurationItem(UiControlOption.InInstanceContentDutyListDisp)]
        [UiDisplayInfo("Hide duty list during instanced duty", "HUD Settings", "Duty List", true)]
        public bool HudDisplayDutyListInstanceDuty { get; private set; }

        [UiConfigurationItem(UiControlOption.InPublicContentDutyListDisp)]
        [UiDisplayInfo("Hide duty list during non-instanced duty", "HUD Settings", "Duty List", true)]
        public bool HudDisplayDutyListNonInstanceDuty { get; private set; }

        [UiConfigurationItem(UiControlOption.ContentsInfoJoiningRequestDisp)]
        [UiDisplayInfo("Display registered duties in Timers", "HUD Settings", "Duty List", true)]
        public bool HudDisplayRegisteredDutiesTimers { get; private set; }

        [UiConfigurationItem(UiControlOption.ContentsInfoJoiningRequestSituationDisp)]
        [UiDisplayInfo("Display duty registration details in Timers", "HUD Settings", "Duty List", true)]
        public bool HudDisplayRegisteredDutyDetailTimers { get; private set; }

        [UiConfigurationItem(UiControlOption.NaviMapDisp)]
        [UiDisplayInfo("Display minimap", "HUD Settings")]
        public bool HudDisplayMinimap { get; private set; }

        [UiConfigurationItem(UiControlOption.GilStatusDisp)]
        [UiDisplayInfo("Display gil", "HUD Settings")]
        public bool HudDisplayGil { get; private set; }

        [UiConfigurationItem(UiControlOption.InfoSettingDisp)]
        [UiDisplayInfo("Display server info", "HUD Settings", "Server Info Bar")]
        public bool HudDisplayServerInfo { get; private set; }

        [UiConfigurationItem(UiControlOption.InfoSettingDispType)]
        [UiDisplayInfo("Clock Type", "HUD Settings", "Server Info Bar")]
        public ServerClockType HudDisplayServerClockType { get; private set; }

        [UiConfigurationItem(UiConfigOption.TimeEorzea)]
        [UiDisplayInfo("Eorzea Time", "HUD Settings", "Server Info Bar", true)]
        public bool HudDisplayServerInfoEorzeaTime { get; set; }

        [UiConfigurationItem(UiConfigOption.TimeLocal)]
        [UiDisplayInfo("Local Time", "HUD Settings", "Server Info Bar", true)]
        public bool HudDisplayServerInfoLocalTime { get; set; }

        [UiConfigurationItem(UiConfigOption.TimeServer)]
        [UiDisplayInfo("Server Time", "HUD Settings", "Server Info Bar", true)]
        public bool HudDisplayServerInfoServerTIme { get; set; }

        [UiConfigurationItem(UiConfigOption.InfoSettingDispWorldNameType)]
        [UiDisplayInfo("Display current World nameKey", "HUD Settings", "Server Info Bar", true)]
        public bool HudDisplayServerInfoCurrentWorld { get; set; }

        [UiConfigurationItem(UiControlOption.LimitBreakGaugeDisp)]
        [UiDisplayInfo("Display limit gauge", "HUD Settings")]
        public bool HudDisplayLimitGauge { get; private set; }

        [UiConfigurationItem(UiControlOption.ScenarioTreeCompleteDisp)]
        [UiDisplayInfo("Display Main Scenario Guide", "HUD Settings", "Main Scenario")]
        public bool HudDisplayScenarioInfo { get; private set; }

        [UiConfigurationItem(UiControlOption.ScenarioTreeCompleteDisp)]
        [UiDisplayInfo("Hide when all quests complete", "HUD Settings", "Main Scenario", true)]
        public bool HudHideScenarioComplete { get; private set; }

        [UiConfigurationItem(UiConfigOption.BattleTalkShowFace)]
        [UiDisplayInfo("Display character portraits with battle dialogue widget", "HUD Settings")]
        public bool HudDisplayBattleTextPortraits { get; private set; }

        [UiConfigurationItem(UiControlOption.PartyListDisp)]
        [UiDisplayInfo("Display party list", "HUD Settings", "Party List")]
        public bool HudDisplayPartyList { get; private set; }

        [UiConfigurationItem(UiControlOption.PartyListSoloOff)]
        [UiDisplayInfo("Hide party list when solo", "HUD Settings", "Party List", true)]
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
                if (configOptionAttribute != null)
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
                if (configOptionAttribute != null)
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
                    if (property != null)
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
