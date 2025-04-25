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
    internal sealed class SystemConfiguration : IGameConfiguration<SystemConfiguration>, ICloneable
    {
        [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
        internal sealed class ConfigurationItemAttribute(
            string name,
            string interfaceGroup,
            SystemConfigOption configOption,
            string headerName = "",
            bool nested = false
        ) : Attribute, IUiDisplay
        {
            /// <inheritdoc />
            public string InterfaceName { get; } = name;

            /// <inheritdoc />
            public string InterfaceHeaderName { get; } = headerName;

            /// <inheritdoc />
            public string InterfaceGroup { get; } = interfaceGroup;

            /// <inheritdoc />
            public bool Nested { get; } = nested;

            /// <summary>
            ///     Enum value for Dalamud's internal handling of this config value.
            ///     This property will be loaded from and applied the setting mapped to this value.
            /// </summary>
            public SystemConfigOption ConfigOption { get; } = configOption;
        }

        public enum CharacterObjectQuantity : ushort
        {
            Maximum = 0,
            High = 1,
            Normal = 2,
            Low = 3,
            Minimum = 4,
        }

#pragma warning disable CS8618
        // Available Configuration Fields
        // Do not change the Property Name of these, it'll break a configuration file.

        [ConfigurationItem(
            name: "Master",
            interfaceGroup: "Sound Settings",
            configOption: SystemConfigOption.SoundMaster,
            headerName: "Volume Settings")]
        [ConfigurationInputRange(0, 100)]
        public uint MasterVolume { get; private set; }

        [ConfigurationItem(
            name: "Background Music",
            interfaceGroup: "Sound Settings",
            configOption: SystemConfigOption.SoundBgm,
            headerName: "Volume Settings")]
        [ConfigurationInputRange(0, 100)]
        public uint BGMVolume { get; private set; }

        [ConfigurationItem(
            name: "Sound Effects",
            interfaceGroup: "Sound Settings",
            configOption: SystemConfigOption.SoundSe,
            headerName: "Volume Settings")]
        [ConfigurationInputRange(0, 100)]
        public uint SoundEffectVolume { get; private set; }

        [ConfigurationItem(
            name: "Voices",
            interfaceGroup: "Sound Settings",
            configOption: SystemConfigOption.SoundVoice,
            headerName: "Volume Settings")]
        [ConfigurationInputRange(0, 100)]
        public uint VoiceVolume { get; private set; }

        [ConfigurationItem(
            name: "System Sounds",
            interfaceGroup: "Sound Settings",
            configOption: SystemConfigOption.SoundSystem,
            headerName: "Volume Settings")]
        [ConfigurationInputRange(0, 100)]
        public uint SystemSoundsVolume { get; private set; }

        [ConfigurationItem(
            name: "Ambient Sounds",
            interfaceGroup: "Sound Settings",
            configOption: SystemConfigOption.SoundEnv,
            headerName: "Volume Settings")]
        [ConfigurationInputRange(0, 100)]
        public uint AmbientSoundsVolume { get; private set; }

        [ConfigurationItem(
            name: "Performance Sounds",
            interfaceGroup: "Sound Settings",
            configOption: SystemConfigOption.SoundPerform,
            headerName: "Volume Settings")]
        [ConfigurationInputRange(0, 100)]
        public uint PerformanceSoundsVolume { get; private set; }

        [ConfigurationItem(
            name: "Player Effects (Self)",
            interfaceGroup: "Sound Settings",
            configOption: SystemConfigOption.SoundPlayer,
            headerName: "Volume Settings")]
        [ConfigurationInputRange(0, 100)]
        public uint PlayerEffectsSelfVolume { get; private set; }

        [ConfigurationItem(
            name: "Player Effects (Party)",
            interfaceGroup: "Sound Settings",
            configOption: SystemConfigOption.SoundParty,
            headerName: "Volume Settings")]
        [ConfigurationInputRange(0, 100)]
        public uint PlayerEffectsPartyVolume { get; private set; }

        [ConfigurationItem(
            name: "Player Effects (Other PCs)",
            interfaceGroup: "Sound Settings",
            configOption: SystemConfigOption.SoundOther,
            headerName: "Volume Settings")]
        [ConfigurationInputRange(0, 100)]
        public uint PlayerEffectsOtherPCsVolume { get; private set; }

        [ConfigurationItem(
            name: "DualSense Speaker Volume",
            interfaceGroup: "Sound Settings",
            configOption: SystemConfigOption.SoundPad,
            headerName: "Dualsense/DUALSHOCK 4 Settings")]
        [ConfigurationInputRange(0, 100)]
        public uint DualSenseSpeakerVolume { get; private set; }

        [ConfigurationItem(
            name: "Character and Object Quantity",
            interfaceGroup: "Other Settings",
            configOption: SystemConfigOption.DisplayObjectLimitType,
            headerName: "Display Limits")]
        public CharacterObjectQuantity CutsceneAndObjectQuantity { get; private set; }

        [ConfigurationItem(
            name: "##ScreenshotFolder",
            interfaceGroup: "Other Settings",
            configOption: SystemConfigOption.ScreenShotDir,
            headerName: "Screenshot Folder")]
        [ConfigurationInputRange(20, 200)]
        public string ScreenshotLocationDir { get; private set; }
#pragma warning restore CS8618

        private readonly HashSet<string> persistedProperties = [];

        /// <inheritdoc />
        public SystemConfiguration PersistAllValues()
        {
            foreach (var prop in typeof(SystemConfiguration).GetProperties())
            {
                this.persistedProperties.Add(prop.Name);
            }
            return this;
        }

        /// <inheritdoc />
        public SystemConfiguration DepersistAllValues()
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
            return JsonConvert.DeserializeObject<SystemConfiguration>(serializedObject)!;
        }

        /// <summary>
        ///     Create an instance of this configuration from the current game settings.
        /// </summary>
        public static SystemConfiguration FromGame()
        {
            var systemConfiguration = new SystemConfiguration();
            foreach (var prop in typeof(SystemConfiguration).GetProperties())
            {
                var configOptionAttribute = prop.GetCustomAttribute<ConfigurationItemAttribute>();
                if (configOptionAttribute != null)
                {
                    if (prop.PropertyType.IsEnum && Plugin.GameConfig.TryGet(configOptionAttribute.ConfigOption, out uint enumValue))
                    {
                        var enumConvertedValue = (Enum)Enum.ToObject(prop.PropertyType, enumValue);
                        prop.SetValue(systemConfiguration, enumConvertedValue);
                    }
                    if (prop.PropertyType == typeof(uint) && Plugin.GameConfig.TryGet(configOptionAttribute.ConfigOption, out uint uintValue))
                    {
                        prop.SetValue(systemConfiguration, uintValue);
                    }
                    else if (prop.PropertyType == typeof(bool) && Plugin.GameConfig.TryGet(configOptionAttribute.ConfigOption, out bool boolValue))
                    {
                        prop.SetValue(systemConfiguration, boolValue);
                    }
                    else if (prop.PropertyType == typeof(string) && Plugin.GameConfig.TryGet(configOptionAttribute.ConfigOption, out string stringValue))
                    {
                        prop.SetValue(systemConfiguration, stringValue);
                    }
                }
            }
            return systemConfiguration;
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
                    if (prop.PropertyType.IsEnum)
                    {
                        var enumValue = Convert.ToUInt32(value);
                        Plugin.GameConfig.Set(configOptionAttribute.ConfigOption, enumValue);
                    }
                    if (prop.PropertyType == typeof(uint))
                    {
                        Plugin.GameConfig.Set(configOptionAttribute.ConfigOption, (uint)value);
                    }
                    else if (prop.PropertyType == typeof(bool))
                    {
                        Plugin.GameConfig.Set(configOptionAttribute.ConfigOption, (bool)value);
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        Plugin.GameConfig.Set(configOptionAttribute.ConfigOption, (string)value);
                    }
                }
            }
        }

        private sealed class JsonConverter : JsonConverter<SystemConfiguration>
        {
            public override void WriteJson(JsonWriter writer, SystemConfiguration? value, JsonSerializer serializer)
            {
                if (value is null)
                {
                    throw new InvalidOperationException("Attempt to write null value");
                }
                var onlyPersisted = new JObject();
                foreach (var prop in value.GetType().GetProperties())
                {
                    var currentValue = prop.GetValue(value);
                    if (value.IsPropertyPersistent(prop) && currentValue is not null)
                    {
                        Plugin.Log.Debug($"Serialising persisted property {prop.Name} ({prop.MemberType}) on SystemConfiguration");
                        onlyPersisted.Add(prop.Name, JToken.FromObject(currentValue));
                    }
                }
                onlyPersisted.WriteTo(writer);

            }

            public override SystemConfiguration ReadJson(JsonReader reader, Type objectType, SystemConfiguration? existingValue, bool hasExistingValue, JsonSerializer serializer)
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
                        Plugin.Log.Debug($"Deserialising persisted property {propertyInfo.Name} ({propertyInfo.MemberType}) on SystemConfiguration");
                    }
                }
                return existingValue;
            }
        }
    }

}
