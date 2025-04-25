using System;
using System.Collections.Generic;
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
                bool nested = false
            )
            {
                this.InterfaceName = name;
                this.InterfaceGroup = interfaceGroup;
                this.ConfigOption = configOption;
                this.InterfaceHeaderName = headerName;
                this.Nested = nested;
            }

            // Constructor for UiControlOption
            public ConfigurationItemAttribute(
                string name,
                string interfaceGroup,
                UiControlOption controlOption,
                string headerName = "",
                bool nested = false
            )
            {
                this.InterfaceName = name;
                this.InterfaceGroup = interfaceGroup;
                this.ControlOption = controlOption;
                this.InterfaceHeaderName = headerName;
                this.Nested = nested;
            }

            /// <inheritdoc />
            public string InterfaceName { get; }

            /// <inheritdoc />
            public string InterfaceHeaderName { get; }

            /// <inheritdoc />
            public string InterfaceGroup { get; }

            /// <inheritdoc />
            public bool Nested { get; }

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
            nested: true
        )]
        public bool ShopConfirmMeldableItems { get; private set; }

        [ConfigurationItem(
            name: "Spiritbound Items",
            interfaceGroup: "Item Settings",
            configOption: UiConfigOption.ShopConfirmSpiritBondMax,
            headerName: "Shop Settings",
            nested: true
        )]
        public bool ShopConfirmSpiritbondMax { get; private set; }

        [ConfigurationItem(
            name: "Unique/Untradeable Items",
            interfaceGroup: "Item Settings",
            configOption: UiConfigOption.ShopConfirmExRare,
            headerName: "Shop Settings",
            nested: true
        )]
        public bool ShopConfirmRareItem { get; private set; }

#pragma warning restore CS8618

        private readonly HashSet<string> persistedProperties = [];

        /// <inheritdoc />
        public UiConfiguration PersistAllValues()
        {
            foreach (var prop in typeof(UiConfiguration).GetProperties())
            {
                this.persistedProperties.Add(prop.Name);
            }
            return this;
        }

        /// <inheritdoc />
        public UiConfiguration DepersistAllValues()
        {
            this.persistedProperties.Clear();
            return this;
        }

        /// <inheritdoc />
        public bool IsPropertyPersistent(PropertyInfo prop) => this.persistedProperties.Contains(prop.Name);

        /// <inheritdoc />
        public void SetPropertyPersistent<T>(PropertyInfo prop, T value)
        {
            if (!prop.CanWrite)
            {
                throw new InvalidOperationException($"Property {prop} is read-only.");
            }
            prop.SetValue(this, value);
            this.persistedProperties.Add(prop.Name);
        }

        /// <inheritdoc />
        public void SetProperty<T>(PropertyInfo prop, T value)
        {
            if (!prop.CanWrite)
            {
                throw new InvalidOperationException($"Property {prop} is read-only.");
            }
            prop.SetValue(this, value);
            this.persistedProperties.Remove(prop.Name);
        }

        /// <inheritdoc />
        public object Clone()
        {
            // Jank to get deep clone.
            var serializedObject = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<UiConfiguration>(serializedObject)!;
        }

        /// <summary>
        ///     Apply all settings in this instance to the current game settings.
        /// </summary>
        public static UiConfiguration FromGame()
        {
            var uiConfiguration = new UiConfiguration();

            foreach (var prop in typeof(UiConfiguration).GetProperties())
            {
                var configOptionAttribute = prop.GetCustomAttribute<ConfigurationItemAttribute>();
                if (configOptionAttribute != null)
                {
                    if (configOptionAttribute.ConfigOption.HasValue)
                    {
                        var configOption = configOptionAttribute.ConfigOption.Value;

                        if (prop.PropertyType.IsEnum && Plugin.GameConfig.TryGet(configOption, out uint enumValue))
                        {
                            var enumConvertedValue = (Enum)Enum.ToObject(prop.PropertyType, enumValue);
                            prop.SetValue(uiConfiguration, enumConvertedValue);
                        }
                        else if (prop.PropertyType == typeof(uint) && Plugin.GameConfig.TryGet(configOption, out uint uintValue))
                        {
                            prop.SetValue(uiConfiguration, uintValue);
                        }
                        else if (prop.PropertyType == typeof(bool) && Plugin.GameConfig.TryGet(configOption, out bool boolValue))
                        {
                            prop.SetValue(uiConfiguration, boolValue);
                        }
                        else if (prop.PropertyType == typeof(string) && Plugin.GameConfig.TryGet(configOption, out string stringValue))
                        {
                            prop.SetValue(uiConfiguration, stringValue);
                        }
                    }
                    else if (configOptionAttribute.ControlOption.HasValue)
                    {
                        var controlOption = configOptionAttribute.ControlOption.Value;
                        if (prop.PropertyType.IsEnum && Plugin.GameConfig.TryGet(controlOption, out uint enumValue))
                        {
                            var enumConvertedValue = (Enum)Enum.ToObject(prop.PropertyType, enumValue);
                            prop.SetValue(uiConfiguration, enumConvertedValue);
                        }
                        else if (prop.PropertyType == typeof(uint) && Plugin.GameConfig.TryGet(controlOption, out uint uintValue))
                        {
                            prop.SetValue(uiConfiguration, uintValue);
                        }
                        else if (prop.PropertyType == typeof(bool) && Plugin.GameConfig.TryGet(controlOption, out bool boolValue))
                        {
                            prop.SetValue(uiConfiguration, boolValue);
                        }
                        else if (prop.PropertyType == typeof(string) && Plugin.GameConfig.TryGet(controlOption, out string stringValue))
                        {
                            prop.SetValue(uiConfiguration, stringValue);
                        }
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
            foreach (var prop in this.GetType().GetProperties())
            {
                if (!this.persistedProperties.Contains(prop.Name))
                {
                    continue;
                }
                var configOptionAttribute = prop.GetCustomAttribute<ConfigurationItemAttribute>();
                if (configOptionAttribute != null)
                {
                    var value = prop.GetValue(this);
                    if (value is null)
                    {
                        continue;
                    }
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
                foreach (var property in value.GetType().GetProperties())
                {
                    var currentValue = property.GetValue(value);
                    if (value.IsPropertyPersistent(property) && currentValue is not null)
                    {
                        Plugin.Log.Debug($"Serialising persisted property {property.Name} ({property.MemberType}) on UiConfiguration");
                        onlyPersisted.Add(property.Name, JToken.FromObject(currentValue));
                    }
                }
                onlyPersisted.WriteTo(writer);
            }

            public override UiConfiguration ReadJson(JsonReader reader, Type objectType, UiConfiguration? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                existingValue = FromGame();
                var jObject = JObject.Load(reader);
                foreach (var property in jObject.Properties())
                {
                    var propertyInfo = objectType.GetProperty(property.Name);
                    if (propertyInfo != null)
                    {
                        var value = property.Value.ToObject(propertyInfo.PropertyType, serializer);
                        propertyInfo.SetValue(existingValue, value);
                        existingValue.persistedProperties.Add(propertyInfo.Name);
                        Plugin.Log.Debug($"Deserialising persisted property {propertyInfo.Name} ({propertyInfo.MemberType}) on UiConfiguration");
                    }
                }
                return existingValue;
            }
        }
    }
}
