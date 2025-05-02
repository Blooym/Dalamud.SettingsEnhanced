using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Game.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SettingsEnhanced.Game.Settings.Attributes;
using SettingsEnhanced.Game.Settings.Interfaces;
using SettingsEnhanced.Game.Settings.Util;

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
            bool indented = false
        ) : Attribute, IUiDisplay
        {
            /// <inheritdoc />
            public string InterfaceName { get; } = name;

            /// <inheritdoc />
            public string InterfaceHeaderName { get; } = headerName;

            /// <inheritdoc />
            public string InterfaceGroup { get; } = interfaceGroup;

            /// <inheritdoc />
            public bool Indented { get; } = indented;

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

        /// <summary>
        ///     Properties that will be kept across serialisation and deserialisation.
        /// </summary>
        private readonly HashSet<string> persistedProperties = [];

        /// <summary>
        ///     Properties that have been modified since the last time this configuration was applied.
        /// </summary>
        private readonly HashSet<string> modifiedProperties = [];

        /// <inheritdoc />
        public SystemConfiguration PersistAllProperties()
        {
            foreach (var prop in typeof(SystemConfiguration).GetProperties(Plugin.ConfigReflectionBindingFlags))
            {
                this.persistedProperties.Add(prop.Name);
            }
            return this;
        }

        /// <inheritdoc />
        public SystemConfiguration DepersistAllProperties()
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
            return JsonConvert.DeserializeObject<SystemConfiguration>(serializedObject)!;
        }

        /// <summary>
        ///     Read all settings from the game into a new instance of this class.
        /// </summary>
        public static SystemConfiguration FromGame()
        {
            var systemConfiguration = new SystemConfiguration();
            foreach (var prop in typeof(SystemConfiguration).GetProperties(Plugin.ConfigReflectionBindingFlags))
            {
                var configOptionAttribute = prop.GetCustomAttribute<ConfigurationItemAttribute>();
                if (configOptionAttribute != null && GameConfigUtil.TryGetGameConfigValue(configOptionAttribute.ConfigOption, prop.PropertyType, out var value))
                {
                    prop.SetValue(systemConfiguration, value);
                }
            }
            return systemConfiguration;
        }

        /// <summary>
        ///     Apply all settings in this instance to the current game settings.
        /// </summary>
        /// <param name="onlyApplyModified">Only apply properties that have been modified since the last time this configuration was applied.</param>
        public void ApplyToGame(bool onlyApplyModified)
        {
            foreach (var prop in typeof(SystemConfiguration)
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
                    this.modifiedProperties.Remove(prop.Name);
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
                foreach (var prop in typeof(SystemConfiguration).GetProperties(Plugin.ConfigReflectionBindingFlags))
                {
                    var currentValue = prop.GetValue(value);
                    if (value.IsPropertyPersisted(prop) && currentValue is not null)
                    {
                        Plugin.Log.Verbose($"Serialising persisted property {prop.Name} ({prop.MemberType}) on SystemConfiguration");
                        onlyPersisted.Add(prop.Name, JToken.FromObject(currentValue));
                    }
                }
                onlyPersisted.WriteTo(writer);

            }

            public override SystemConfiguration ReadJson(JsonReader reader, Type objectType, SystemConfiguration? existingValue, bool hasExistingValue, JsonSerializer serializer)
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
                        Plugin.Log.Verbose($"Deserialising persisted property {property.Name} ({property.MemberType}) on SystemConfiguration");
                    }
                }
                return existingValue;
            }
        }
    }

}
