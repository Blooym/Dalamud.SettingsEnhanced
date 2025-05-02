using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Game.Config;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using SettingsEnhanced.Configuration;
using SettingsEnhanced.Game.Settings;
using SettingsEnhanced.Game.Settings.Attributes;
using SettingsEnhanced.Game.Settings.Util;
using SettingsEnhanced.Resources.Localization;
using SettingsEnhanced.UI;
using SettingsEnhanced.UI.Windows;
using SystemConfiguration = SettingsEnhanced.Game.Settings.SystemConfiguration;

namespace SettingsEnhanced
{

    internal sealed class Plugin : IDalamudPlugin
    {
        // SAFETY: valid from plugin start.
#pragma warning disable CS8618
        [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; }
        [PluginService] public static IClientState ClientState { get; set; }
        [PluginService] public static IPluginLog Log { get; set; }
        [PluginService] public static IDataManager DataManager { get; set; }
        [PluginService] public static IGameConfig GameConfig { get; set; }
        [PluginService] public static INotificationManager NotificationManager { get; set; }
        private static WindowManager WindowManager { get; set; }
        private static LocalizationManager LocalizationManager { get; set; }
        public static PluginConfiguration PluginConfiguration { get; private set; }
        public static IEnumerable<TerritoryType> AllowedTerritories;
#pragma warning restore CS8618

        private const uint NotificationShowSeconds = 4;
        private static readonly uint[] AllowedTerritoryUse = [
             0, // Town
             1 ,// Open World
             2 ,// Inn
             13, // Housing Area
             19, // Chocobo Square
             23, // Gold Saucer
             30, // Free Company Garrison
             33, // Treasure Map Instance
             41, // Eureka
             44, // Leap of faith
             45, // Masked Carnival
             46, // Ocean Fishing
             48, // Bozja
             60, // Cosmic Exploration
        ];
        public const BindingFlags ConfigReflectionBindingFlags = BindingFlags.Public | BindingFlags.Instance;
        public static ulong CurrentPlayerContentId { get; private set; }

        public Plugin()
        {
            LocalizationManager = new();
            AllowedTerritories = DataManager.Excel.GetSheet<TerritoryType>().Where(x => AllowedTerritoryUse.Contains(x.TerritoryIntendedUse.RowId) && !x.IsPvpZone);
            PluginConfiguration = PluginConfiguration.Load();

            // Warn about being left in a bad state
            if (PluginConfiguration.SystemConfigurationOverwritten || PluginConfiguration.UiConfigurationOverwritten)
            {
                Log.Warning($"Configuration data was not set back to default last plugin shutdown, possible game crash? SystemOverwritten: {PluginConfiguration.SystemConfigurationOverwritten} UIOverwritten: {PluginConfiguration.UiConfigurationOverwritten}");
            }

            ClientState.Login += this.OnLogin;
            ClientState.Logout += this.OnLogout;
            ClientState.TerritoryChanged += this.UpdateGameSettingsForTerritory;
            GameConfig.SystemChanged += OnSystemConfigUpdated;
            GameConfig.UiConfigChanged += this.OnUiConfigChanged;
            WindowManager = new();
            ConfigurationWindow.ConfigurationUpdated += this.OnConfigurationWindowSave;
            PluginConfiguration.WriteNewSysConfigOriginalSafe();
            if (ClientState.IsLoggedIn)
            {
                CurrentPlayerContentId = ClientState.LocalContentId;
                GameConfig.UiConfigChanged += this.OnUiConfigChanged;
                GameConfig.UiControlChanged += this.OnUiConfigChanged;
                PluginConfiguration.WriteNewUiConfigOriginalSafe(CurrentPlayerContentId);
                Log.Information($"Plugin enabled whilst player logged in: triggering manual territory update");
                this.UpdateGameSettingsForTerritory(ClientState.TerritoryType);
            }
        }

        public void Dispose()
        {
            GameConfig.SystemChanged -= OnSystemConfigUpdated;
            GameConfig.UiConfigChanged -= this.OnUiConfigChanged;
            GameConfig.UiControlChanged -= this.OnUiControlChanged;
            ClientState.Login -= this.OnLogin;
            ClientState.Logout -= this.OnLogout;
            ClientState.TerritoryChanged -= this.UpdateGameSettingsForTerritory;
            ConfigurationWindow.ConfigurationUpdated -= this.OnConfigurationWindowSave;
            WindowManager.Dispose();

            // Apply the base game settings again.
            RestoreAllGameSettings();

            LocalizationManager.Dispose();
        }

        /// <summary>
        ///     Handles setting the current player content id value.
        /// </summary>
        private void OnLogin()
        {
            CurrentPlayerContentId = ClientState.LocalContentId;
            GameConfig.UiConfigChanged += this.OnUiConfigChanged;
            GameConfig.UiControlChanged += this.OnUiControlChanged;
            PluginConfiguration.WriteNewUiConfigOriginalSafe(CurrentPlayerContentId);
        }

        /// <summary>
        ///     Handles restoring base game settings on logout/game close and unsetting
        ///     the current player content id value.
        /// </summary>
        private void OnLogout(int type, int code)
        {
            RestoreAllGameSettings();
            CurrentPlayerContentId = 0;
            GameConfig.UiControlChanged -= this.OnUiControlChanged;
            GameConfig.UiConfigChanged -= this.OnUiConfigChanged;
        }

        /// <summary>
        ///     Manually trigger configuration settings to reapply.
        /// </summary>
        private void OnConfigurationWindowSave() => this.UpdateGameSettingsForTerritory(ClientState.TerritoryType);

        /// <summary>
        ///     Applies specific zone overrides to settings or handles restoring them when leaving overriden zones.
        /// </summary>
        /// <param name="territoryId"></param>
        private void UpdateGameSettingsForTerritory(ushort territoryId)
        {
            var didApplyType = 0;

            Log.Debug($"Checking if plugin should overwrite or restore game settings data for {territoryId}");

            // Explicitly remove configurations for territories that aren't allowed.
            if (AllowedTerritories.All(x => territoryId != x.RowId))
            {
                Log.Warning($"Configuration contains a territory override for {territoryId} which isn't in the allowlist, it will be removed");
                PluginConfiguration.TerritorySystemConfiguration.Remove(territoryId);
                PluginConfiguration.TerritorySystemConfiguration.Remove(territoryId);
                PluginConfiguration.Save();
            }

            // Modify or restore system configuration data.
            if (PluginConfiguration.TerritorySystemConfiguration.TryGetValue(territoryId, out var systemConfig))
            {
                Log.Information($"{territoryId} has system setting overrides, applying modified data");
                systemConfig.ApplyToGame(onlyApplyModified: true);
                PluginConfiguration.SystemConfigurationOverwritten = true;
                PluginConfiguration.Save();
                didApplyType = 1;
            }
            else if (PluginConfiguration.SystemConfigurationOverwritten)
            {
                Log.Information($"{territoryId} does not have any system setting overrides and they are still modified, restoring game defaults");
                PluginConfiguration.OriginalSystemConfiguration.ApplyToGame(onlyApplyModified: false);
                PluginConfiguration.SystemConfigurationOverwritten = false;
                PluginConfiguration.Save();
                didApplyType = 2;
            }

            // Modify or restore ui configuration data.
            if (PluginConfiguration.TerritoryUiConfiguration.TryGetValue(territoryId, out var uiConfig))
            {
                Log.Information($"{territoryId} has ui settings overrides, applying modified data");
                uiConfig.ApplyToGame(onlyApplyModified: true);
                PluginConfiguration.UiConfigurationOverwritten = true;
                PluginConfiguration.Save();
                didApplyType = 1;
            }
            else if (PluginConfiguration.UiConfigurationOverwritten && PluginConfiguration.OriginalUiConfiguration.TryGetValue(CurrentPlayerContentId, out var charConfig))
            {
                Log.Information($"{territoryId} does not have any ui settings overrides and they are still modified, restoring game defaults");
                charConfig.ApplyToGame(onlyApplyModified: false);
                PluginConfiguration.UiConfigurationOverwritten = false;
                PluginConfiguration.Save();
                didApplyType = 2;
            }

            // Notify depending on state changes.
            switch (didApplyType)
            {
                case 1:
                    NotificationManager.AddNotification(new Notification()
                    {
                        Title = Strings.Notification_ConfigurationModified_Title,
                        Content = Strings.Notification_ConfigurationModified_Content,
                        HardExpiry = DateTime.Now.AddSeconds(NotificationShowSeconds),
                        Type = NotificationType.Info
                    });
                    break;
                case 2:
                    NotificationManager.AddNotification(new Notification()
                    {
                        Title = Strings.Notification_ConfigurationRestored_Title,
                        Content = Strings.Notification_ConfigurationRestored_Content,
                        HardExpiry = DateTime.Now.AddSeconds(NotificationShowSeconds),
                        Type = NotificationType.Info
                    });
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     Updates stored original system configuration when configuration is overwritten.
        /// </summary>
        private static void OnSystemConfigUpdated(object? sender, ConfigChangeEvent e)
        {
            // Only handles options the plugin controls.
            var option = (SystemConfigOption)e.Option;
            var updatedOptionProperty = typeof(SystemConfiguration)
                .GetProperties(ConfigReflectionBindingFlags)
                .FirstOrDefault(x => x.GetCustomAttribute<SystemConfigurationItemAttribute>()?.ConfigOption == option);
            if (updatedOptionProperty is null)
            {
                return;
            }
            GameConfigUtil.TryGetGameConfigValue(option, updatedOptionProperty.PropertyType, out var value);
            PluginConfiguration.TerritorySystemConfiguration.TryGetValue(ClientState.TerritoryType, out var config);

            // If the plugin has not overwritten settings OR if the current configuration has not overwritten this option, update the original config.
            if (!PluginConfiguration.SystemConfigurationOverwritten || (config is not null && !config.IsPropertyPersisted(updatedOptionProperty)))
            {
                Log.Debug($"SystemConfiguration saving {option} to original value as value is not overwritten");
                PluginConfiguration.OriginalSystemConfiguration.SetPropertyValue(updatedOptionProperty, value);
                PluginConfiguration.Save();
                return;
            }
            // If the plugin has a config for this area and is overwriting the option, don't apply the change anywhere
            if (config is not null && config.IsPropertyPersisted(updatedOptionProperty))
            {
                Log.Debug($"SystemConfiguration not saving {option} as it is overwritten by the plugin");
                return;
            }
        }

        /// <summary>
        ///     Updates stored original ui configuration when configuration is overwritten.
        /// </summary>
        private void OnUiConfigChanged(object? sender, ConfigChangeEvent e)
        {
            if (CurrentPlayerContentId == 0)
            {
                return;
            }

            // Only handles options the plugin controls.
            var option = (UiConfigOption)e.Option;
            var updatedOptionProperty = typeof(UiConfiguration)
                .GetProperties(ConfigReflectionBindingFlags)
                .FirstOrDefault(x => x.GetCustomAttribute<UiConfigurationItemAttribute>()?.ConfigOption == option);
            if (updatedOptionProperty is null)
            {
                return;
            }

            GameConfigUtil.TryGetGameConfigValue(option, updatedOptionProperty.PropertyType, out var value);
            PluginConfiguration.TerritoryUiConfiguration.TryGetValue(ClientState.TerritoryType, out var config);

            // If the plugin has not overwritten settings OR if the current configuration has not overwritten this option, update the original config.
            if (!PluginConfiguration.UiConfigurationOverwritten || (config is not null && !config.IsPropertyPersisted(updatedOptionProperty)))
            {
                Log.Debug($"UiConfiguration saving {option} to original value as value is not overwritten");
                PluginConfiguration.OriginalSystemConfiguration.SetPropertyValue(updatedOptionProperty, value);
                PluginConfiguration.Save();
                return;
            }
            // If the plugin has a config for this area and is overwriting the option, don't apply the change anywhere
            if (config is not null && config.IsPropertyPersisted(updatedOptionProperty))
            {
                Log.Debug($"UiConfiguration not saving {option} as it is overwritten by the plugin");
                return;
            }
        }

        /// <summary>
        ///     Updates stored original ui configuration when configuration is overwritten.
        /// </summary>
        private void OnUiControlChanged(object? sender, ConfigChangeEvent e)
        {
            if (CurrentPlayerContentId == 0)
            {
                return;
            }
            // Only handles options the plugin controls.
            var option = (UiControlOption)e.Option;
            var updatedOptionProperty = typeof(UiConfiguration)
                .GetProperties(ConfigReflectionBindingFlags)
                .FirstOrDefault(x => x.GetCustomAttribute<UiConfigurationItemAttribute>()?.ControlOption == option);
            if (updatedOptionProperty is null)
            {
                return;
            }
            GameConfigUtil.TryGetGameConfigValue(option, updatedOptionProperty.PropertyType, out var value);
            PluginConfiguration.TerritoryUiConfiguration.TryGetValue(ClientState.TerritoryType, out var config);

            // If the plugin has not overwritten settings OR if the current configuration has not overwritten this option, update the original config.
            if (!PluginConfiguration.UiConfigurationOverwritten || (config is not null && !config.IsPropertyPersisted(updatedOptionProperty)))
            {
                Log.Debug($"UiConfiguration saving {option} to original value as value is not overwritten");
                PluginConfiguration.OriginalSystemConfiguration.SetPropertyValue(updatedOptionProperty, value);
                PluginConfiguration.Save();
                return;
            }
            // If the plugin has a config for this area and is overwriting the option, don't apply the change anywhere
            if (config is not null && config.IsPropertyPersisted(updatedOptionProperty))
            {
                Log.Debug($"UiConfiguration not saving {option} as it is overwritten by the plugin");
                return;
            }
        }

        /// <summary>
        ///     Restores all game configuration values to their original settings.
        /// </summary>
        private static void RestoreAllGameSettings()
        {
            Log.Information($"Restoring all game settings to original values");

            var anyRestored = false;
            if (PluginConfiguration.UiConfigurationOverwritten && PluginConfiguration.OriginalUiConfiguration.TryGetValue(CurrentPlayerContentId, out var charConfig))
            {
                charConfig.ApplyToGame(onlyApplyModified: false);
                PluginConfiguration.UiConfigurationOverwritten = false;
                anyRestored = true;
                Log.Information($"Restored UiConfiguration for {CurrentPlayerContentId}");
            }
            if (PluginConfiguration.SystemConfigurationOverwritten)
            {
                PluginConfiguration.OriginalSystemConfiguration.ApplyToGame(onlyApplyModified: false);
                PluginConfiguration.SystemConfigurationOverwritten = false;
                anyRestored = true;
                Log.Information($"Restored SystemConfiguration");
            }
            if (anyRestored)
            {
                PluginConfiguration.Save();
                NotificationManager.AddNotification(new Notification()
                {
                    Title = Strings.Notification_ConfigurationRestored_Title,
                    Content = Strings.Notification_ConfigurationRestored_Content,
                    HardExpiry = DateTime.Now.AddSeconds(NotificationShowSeconds),
                    Type = NotificationType.Info
                });
            }
        }
    }
}
