using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Game.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SettingsEnhanced.Game.Settings.Attributes;
using SettingsEnhanced.Game.Settings.Interfaces;

namespace SettingsEnhanced.Game.Settings
{

    [JsonConverter(typeof(JsonConverter))]
    internal sealed class UiConfiguration : IGameConfiguration<UiConfiguration>, ICloneable
    {
        [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
        internal sealed class ConfigurationItemAttribute : Attribute, IUiDisplay
        {
            // Constructor for UiConfigOption
            public ConfigurationItemAttribute(
                string name,
                string interfaceGroup,
                UiConfigOption configOption,
                string headerName = "",
                bool indented = false
            )
            {
                this.InterfaceName = name;
                this.InterfaceGroup = interfaceGroup;
                this.ConfigOption = configOption;
                this.InterfaceHeaderName = headerName;
                this.Indented = indented;
            }

            // Constructor for UiControlOption
            public ConfigurationItemAttribute(
                string name,
                string interfaceGroup,
                UiControlOption controlOption,
                string headerName = "",
                bool indented = false
            )
            {
                this.InterfaceName = name;
                this.InterfaceGroup = interfaceGroup;
                this.ControlOption = controlOption;
                this.InterfaceHeaderName = headerName;
                this.Indented = indented;
            }

            /// <inheritdoc />
            public string InterfaceName { get; }

            /// <inheritdoc />
            public string InterfaceHeaderName { get; }

            /// <inheritdoc />
            public string InterfaceGroup { get; }

            /// <inheritdoc />
            public bool Indented { get; }

            // Property for UiConfigOption
            public UiConfigOption? ConfigOption { get; }

            // Property for UiControlOption
            public UiControlOption? ControlOption { get; }
        }

        internal enum MovementType
        {
            Standard = 0,
            Legacy = 1
        }

        internal enum BattleEffects
        {
            All = 0,
            Limited = 1,
            None = 2,
        }

        public enum FlyTextSize
        {
            Standard = 0,
            Large = 1,
            Maximum = 2
        }

        public enum PopupTextSize
        {
            Standard = 0,
            Large = 1,
            Maximum = 2
        }

        public enum ServerClockType
        {
            DefaultToLanguage = 0,
            TwentyFourHour = 1,
            TwelveHour = 2,
        }

#pragma warning disable CS8618
        // Available Configuration Fields
        // Do not change the Property Name of these, it'll break a configuration file.

        [ConfigurationItem(
            name: "Movement Type",
            interfaceGroup: "Control Settings",
            controlOption: UiControlOption.MoveMode,
            headerName: "Movement Settings")]
        public MovementType MoveMode { get; private set; }

        [ConfigurationItem(
            name: "Own",
            interfaceGroup: "Character Settings",
            configOption: UiConfigOption.BattleEffectSelf,
            headerName: "Battle Effects Settings")]
        public BattleEffects BattleEffectSelf { get; private set; }

        [ConfigurationItem(
            name: "Party",
            interfaceGroup: "Character Settings",
            configOption: UiConfigOption.BattleEffectParty,
            headerName: "Battle Effects Settings")]
        public BattleEffects BattleEffectParty { get; private set; }

        [ConfigurationItem(
            name: "Other (excl. PvP)",
            interfaceGroup: "Character Settings",
            configOption: UiConfigOption.BattleEffectOther,
            headerName: "Battle Effects Settings")]
        public BattleEffects BattleEffectOther { get; private set; }

        [ConfigurationItem(
            name: "PvP Opponents",
            interfaceGroup: "Character Settings",
            configOption: UiConfigOption.BattleEffectPvPEnemyPc,
            headerName: "Battle Effects Settings")]
        public BattleEffects BattleEffectPvPOpponent { get; private set; }

        [ConfigurationItem(
            name: "Idle Animation Delay",
            interfaceGroup: "Character Settings",
            configOption: UiConfigOption.IdleEmoteTime,
            headerName: "Animation")]
        [ConfigurationInputRange(0, 30)]
        public uint IdleAnimationDelay { get; private set; }

        [ConfigurationItem(
            name: "Randomize idle animation",
            interfaceGroup: "Character Settings",
            configOption: UiConfigOption.IdleEmoteRandomType,
            headerName: "Animation")]
        public bool IdleEmoteRandomType { get; private set; }

        [ConfigurationItem(
            name: "Display confirmation when selling items",
            interfaceGroup: "Item Settings",
            configOption: UiConfigOption.ShopConfirm,
            headerName: "Shop Settings")]
        public bool ShopConfirmOnSell { get; private set; }

        [ConfigurationItem(
            name: "Meldable Items",
            interfaceGroup: "Item Settings",
            configOption: UiConfigOption.ShopConfirmMateria,
            headerName: "Shop Settings",
            indented: true
        )]
        public bool ShopConfirmMeldableItems { get; private set; }

        [ConfigurationItem(
            name: "Spiritbound Items",
            interfaceGroup: "Item Settings",
            configOption: UiConfigOption.ShopConfirmSpiritBondMax,
            headerName: "Shop Settings",
            indented: true
        )]
        public bool ShopConfirmSpiritbondMax { get; private set; }

        [ConfigurationItem(
            name: "Unique/Untradeable Items",
            interfaceGroup: "Item Settings",
            configOption: UiConfigOption.ShopConfirmExRare,
            headerName: "Shop Settings",
            indented: true
        )]
        public bool ShopConfirmRareItem { get; private set; }

        [ConfigurationItem(
            name: "Display Emote Log Message",
            interfaceGroup: "Chat Settings",
            configOption: UiConfigOption.EmoteTextType,
            headerName: "Emotes"
         )]
        public bool EmoteDisplayLogMessage { get; private set; }

        [ConfigurationItem(
           name: "Display flying text",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.FlyTextDisp,
           headerName: "Text"
        )]
        public bool HudDisplayFlyingText { get; private set; }

        [ConfigurationItem(
           name: "Flying Text Size",
           interfaceGroup: "HUD Settings",
           configOption: UiConfigOption.FlyTextDispSize,
           headerName: "Text",
           indented: true
        )]
        public FlyTextSize HudFlyingTextSize { get; private set; }

        [ConfigurationItem(
           name: "Display pop-up text",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.PopUpTextDisp,
           headerName: "Text"
        )]
        public bool HudDisplayPopupText { get; private set; }

        [ConfigurationItem(
           name: "Pop-up Text Size",
           interfaceGroup: "HUD Settings",
           configOption: UiConfigOption.FlyTextDispSize,
           headerName: "Text",
           indented: true
        )]
        public PopupTextSize HudPopupTextSize { get; private set; }

        [ConfigurationItem(
           name: "Display parameter bars",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.CharaParamDisp
        )]
        public bool HudDisplayParameterBars { get; private set; }

        [ConfigurationItem(
           name: "Display EXP bar",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.ExpDisp
        )]
        public bool HudDisplayExpBar { get; private set; }

        [ConfigurationItem(
           name: "Display inventory grid",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.InventryStatusDisp
        )]
        public bool HudDisplayInventoryGrid { get; private set; }

        [ConfigurationItem(
           name: "Display duty list",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.DutyListDisp,
           headerName: "Duty List"
        )]
        public bool HudDisplayDutyList { get; private set; }

        [ConfigurationItem(
           name: "Number of Duties Displayed",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.DutyListNumDisp,
           headerName: "Duty List",
           indented: true
        )]
        [ConfigurationInputRange(1, 10)]
        public uint HudDisplayDutyCount { get; private set; }

        [ConfigurationItem(
           name: "Hide duty list during instanced duty",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.InInstanceContentDutyListDisp,
           headerName: "Duty List",
           indented: true
        )]
        public bool HudDisplayDutyListInstanceDuty { get; private set; }

        [ConfigurationItem(
           name: "Hide duty list during non-instanced duty",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.InPublicContentDutyListDisp,
           headerName: "Duty List",
           indented: true
        )]
        public bool HudDisplayDutyListNonInstanceDuty { get; private set; }

        [ConfigurationItem(
           name: "Display registered duties in Timers",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.ContentsInfoJoiningRequestDisp,
           headerName: "Duty List",
           indented: true
        )]
        public bool HudDisplayRegisteredDutiesTimers { get; private set; }

        [ConfigurationItem(
           name: "Display duty registration details in Timers",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.ContentsInfoJoiningRequestSituationDisp,
           headerName: "Duty List",
           indented: true
        )]
        public bool HudDisplayRegisteredDutyDetailTimers { get; private set; }

        [ConfigurationItem(
           name: "Display minimap",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.NaviMapDisp
         )]
        public bool HudDisplayMinimap { get; private set; }

        [ConfigurationItem(
           name: "Display gil",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.GilStatusDisp
        )]
        public bool HudDisplayGil { get; private set; }

        [ConfigurationItem(
           name: "Display server info",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.InfoSettingDisp,
           headerName: "Server Info Bar"
        )]
        public bool HudDisplayServerInfo { get; private set; }

        [ConfigurationItem(
           name: "Clock Type",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.InfoSettingDispType,
           headerName: "Server Info Bar"
        )]
        public ServerClockType HudDisplayServerClockType { get; private set; }

        [ConfigurationItem(
           name: "Eorzea Time",
           interfaceGroup: "HUD Settings",
           configOption: UiConfigOption.TimeEorzea,
           headerName: "Server Info Bar",
           indented: true
        )]
        public bool HudDisplayServerInfoEorzeaTime { get; set; }

        [ConfigurationItem(
           name: "Local Time",
           interfaceGroup: "HUD Settings",
           configOption: UiConfigOption.TimeLocal,
           headerName: "Server Info Bar",
           indented: true
        )]
        public bool HudDisplayServerInfoLocalTime { get; set; }

        [ConfigurationItem(
           name: "Server Time",
           interfaceGroup: "HUD Settings",
           configOption: UiConfigOption.TimeServer,
           headerName: "Server Info Bar",
           indented: true
        )]
        public bool HudDisplayServerInfoServerTIme { get; set; }

        [ConfigurationItem(
           name: "DIsplay current World name",
           interfaceGroup: "HUD Settings",
           configOption: UiConfigOption.InfoSettingDispWorldNameType,
           headerName: "Server Info Bar",
           indented: true
        )]
        public bool HudDisplayServerInfoCurrentWorld { get; set; }

        [ConfigurationItem(
           name: "Display limit gauge",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.LimitBreakGaugeDisp
        )]
        public bool HudDisplayLimitGauge { get; private set; }

        [ConfigurationItem(
           name: "Display Main Scenario Guide",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.ScenarioTreeCompleteDisp,
           headerName: "Main Scenario"
        )]
        public bool HudDisplayScenarioInfo { get; private set; }

        [ConfigurationItem(
           name: "Hide when all quests complete",
           interfaceGroup: "HUD Settings",
           controlOption: UiControlOption.ScenarioTreeCompleteDisp,
           headerName: "Main Scenario",
           indented: true
        )]
        public bool HudHideScenarioComplete { get; private set; }

        [ConfigurationItem(
           name: "Display character portraits with battle dialogue widget",
           interfaceGroup: "HUD Settings",
           configOption: UiConfigOption.BattleTalkShowFace
        )]
        public bool HudDisplayBattleTextPortraits { get; private set; }

        [ConfigurationItem(
            name: "Display party list",
            interfaceGroup: "HUD Settings",
            controlOption: UiControlOption.PartyListDisp,
            headerName: "Party List"
        )]
        public bool HudDisplayPartyList { get; private set; }

        [ConfigurationItem(
            name: "Hide party list when solo",
            interfaceGroup: "HUD Settings",
            controlOption: UiControlOption.PartyListSoloOff,
            headerName: "Party List",
            indented: true
        )]
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
                var configOptionAttribute = prop.GetCustomAttribute<ConfigurationItemAttribute>();
                if (configOptionAttribute != null)
                {
                    if (configOptionAttribute.ConfigOption is not null && GameConfigUtil.TryGetGameConfigValue(configOptionAttribute.ConfigOption, prop.PropertyType, out var uiConfigValue) && uiConfigValue is not null)
                    {
                        prop.SetValue(uiConfiguration, uiConfigValue);
                    }
                    else if (configOptionAttribute.ControlOption is not null && GameConfigUtil.TryGetGameConfigValue(configOptionAttribute.ControlOption, prop.PropertyType, out var uiControlValue) && uiControlValue is not null)
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
                var configOptionAttribute = prop.GetCustomAttribute<ConfigurationItemAttribute>();
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
                        Plugin.Log.Verbose($"Deserialising persisted property {property.Name} ({property.MemberType}) on UiConfiguration");
                    }
                }
                return existingValue;
            }
        }
    }
}
