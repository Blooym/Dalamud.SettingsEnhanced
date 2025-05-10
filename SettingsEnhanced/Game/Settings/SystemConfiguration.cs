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
    internal sealed partial class SystemConfiguration : IGameConfiguration<SystemConfiguration>, ICloneable
    {
#pragma warning disable CS8618
        // Available Configuration Fields
        // Do not change the Property Name of these, it'll break a configuration file.

        [SystemConfigurationItem(SystemConfigOption.IsSoundAlways)]
        [UiSettingPropDisplay(4041, 4171, 0)]
        public bool PlaySoundsWhenInactiveAll { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.IsSoundBgmAlways)]
        [UiSettingPropDisplay(8686, 4171, 0, true)]
        public bool PlaySoundsWhenInactiveBgm { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.IsSoundSeAlways)]
        [UiSettingPropDisplay(8687, 4171, 0, true)]
        public bool PlaySoundsWhenInactiveSfx { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.IsSoundVoiceAlways)]
        [UiSettingPropDisplay(8688, 4171, 0, true)]
        public bool PlaySoundsWhenInactiveVoice { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.IsSoundSystemAlways)]
        [UiSettingPropDisplay(8689, 4171, 0, true)]
        public bool PlaySoundsWhenInactiveSystem { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.IsSoundEnvAlways)]
        [UiSettingPropDisplay(8690, 4171, 0, true)]
        public bool PlaySoundsWhenInactiveEnv { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.IsSoundPerformAlways)]
        [UiSettingPropDisplay(8685, 4171, 0, true)]
        public bool PlaySoundsWhenInactivePerform { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundChocobo)]
        [UiSettingPropDisplay(4042, 4171, 0)]
        public bool PlayMusicWhenMounted { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundFieldBattle)]
        [UiSettingPropDisplay(8733, 4171, 0)]
        public bool EnableNormalBattleMusic { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundHousing)]
        [UiSettingPropDisplay(8734, 4171, 0)]
        public bool EnableCityMusicInResidentialAreas { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundCfTimeCount)]
        [UiSettingPropDisplay(4056, 4171, 0)]
        public bool PlaySystemSoundsWaitingForDutyFinder { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundMicpos)]
        [UiSettingPropDisplay(4043, 4171, 0)]
        [ConfigurationInputRange(0, 100)]
        public uint SoundListeningPosition { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundMaster)]
        [UiSettingPropDisplay(4047, 4171, 4046)]
        [ConfigurationInputRange(0, 100)]
        public uint MasterVolume { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundBgm)]
        [UiSettingPropDisplay(8686, 4171, 4046)]
        [ConfigurationInputRange(0, 100)]
        public uint BgmVolume { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundSe)]
        [UiSettingPropDisplay(8687, 4171, 4046)]
        [ConfigurationInputRange(0, 100)]
        public uint SoundEffectVolume { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundVoice)]
        [UiSettingPropDisplay(8688, 4171, 4046)]
        [ConfigurationInputRange(0, 100)]
        public uint VoiceVolume { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundSystem)]
        [UiSettingPropDisplay(8689, 4171, 4046)]
        [ConfigurationInputRange(0, 100)]
        public uint SystemSoundsVolume { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundEnv)]
        [UiSettingPropDisplay(8690, 4171, 4046)]
        [ConfigurationInputRange(0, 100)]
        public uint AmbientSoundsVolume { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundPerform)]
        [UiSettingPropDisplay(8685, 4171, 4046)]
        [ConfigurationInputRange(0, 100)]
        public uint PerformanceSoundsVolume { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundPlayer)]
        [UiSettingPropDisplay(4127, 4171, 4126)]
        [ConfigurationInputRange(0, 100)]
        public uint PlayerEffectsSelfVolume { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundParty)]
        [UiSettingPropDisplay(4128, 4171, 4126)]
        [ConfigurationInputRange(0, 100)]
        public uint PlayerEffectsPartyVolume { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundOther)]
        [UiSettingPropDisplay(4129, 4171, 4126)]
        [ConfigurationInputRange(0, 100)]
        public uint PlayerEffectsOtherPCsVolume { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundPadSeType)]
        [UiSettingPropDisplay(8203, 4171, 8759)]
        public DualSenseSpeakerSoundType DualSenseSpeakerSoundType { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundPad)]
        [UiSettingPropDisplay(8202, 4171, 8759)]
        [ConfigurationInputRange(0, 100)]
        public uint DualSenseSpeakerVolume { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.SoundEqualizerType)]
        [UiSettingPropDisplay(8737, 4171, 8736)]
        public SoundEqualizerType SoundEqualizerMode { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.DisplayObjectLimitType)]
        [UiSettingPropDisplay(8205, 4177, 8205)]
        public CharacterObjectQuantity CutsceneAndObjectQuantity { get; private set; }

        [SystemConfigurationItem(SystemConfigOption.ScreenShotDir)]
        [UiSettingPropDisplay(0, 4177, 8287)]
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
                var configOptionAttribute = prop.GetCustomAttribute<SystemConfigurationItemAttribute>();
                if (configOptionAttribute is not null && GameConfigUtil.TryGetGameConfigValue(configOptionAttribute.ConfigOption, prop.PropertyType, out var value))
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
                var configOptionAttribute = prop.GetCustomAttribute<SystemConfigurationItemAttribute>();
                if (configOptionAttribute is not null)
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
            /// <inheritdoc />
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

            /// <inheritdoc />
            public override SystemConfiguration ReadJson(JsonReader reader, Type objectType, SystemConfiguration? existingValue, bool hasExistingValue, JsonSerializer serializer)
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
                        existingValue.modifiedProperties.Add(property.Name);
                        property.SetValue(existingValue, value);
                        Plugin.Log.Verbose($"Deserialising persisted property {property.Name} ({property.MemberType}) on SystemConfiguration");
                    }
                }
                return existingValue;
            }
        }
    }

}
