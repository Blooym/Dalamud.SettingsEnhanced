using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Config;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using SettingsEnhanced.Configuration;
using SettingsEnhanced.UI;
using SettingsEnhanced.UI.Windows;

namespace SettingsEnhanced
{

    internal sealed class Plugin : IDalamudPlugin
    {
#pragma warning disable CS8618
        [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; }
        [PluginService] public static IClientState ClientState { get; set; }
        [PluginService] public static IPluginLog Log { get; set; }
        [PluginService] public static IDataManager DataManager { get; set; }
        [PluginService] public static IGameConfig GameConfig { get; set; }
        [PluginService] public static INotificationManager NotificationManager { get; set; }
        public static WindowManager WindowManager { get; private set; }
        public static PluginConfiguration PluginConfiguration { get; private set; }
        public static IEnumerable<TerritoryType> AllowedTerritories;
#pragma warning restore CS8618

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

        public static ulong CurrentPlayerContentId { get; private set; }

        public Plugin()
        {
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
            GameConfig.SystemChanged += this.OnSystemConfigUpdated;
            GameConfig.UiConfigChanged += this.OnUiConfigChanged;
            WindowManager = new();
            ConfigurationWindow.ConfigurationUpdated += this.OnConfigurationWindowSave;
            PluginConfiguration.WriteNewSysConfigOriginalSafe();
            if (ClientState.IsLoggedIn)
            {
                CurrentPlayerContentId = ClientState.LocalContentId;
                GameConfig.UiConfigChanged += this.OnUiConfigChanged;
                PluginConfiguration.WriteNewUiConfigOriginalSafe(CurrentPlayerContentId);
                Log.Information($"Plugin enabled whilst player logged in: triggering manual territory update");
                this.UpdateGameSettingsForTerritory(ClientState.TerritoryType);
            }
        }

        public void Dispose()
        {
            GameConfig.SystemChanged -= this.OnSystemConfigUpdated;
            GameConfig.UiConfigChanged -= this.OnUiConfigChanged;
            ClientState.Login -= this.OnLogin;
            ClientState.Logout -= this.OnLogout;
            ClientState.TerritoryChanged -= this.UpdateGameSettingsForTerritory;
            ConfigurationWindow.ConfigurationUpdated -= this.OnConfigurationWindowSave;
            WindowManager.Dispose();

            // Apply the base game settings again.
            RestoreAllGameSettings();
        }

        /// <summary>
        ///     Handles setting the current player content id value.
        /// </summary>
        public void OnLogin()
        {
            CurrentPlayerContentId = ClientState.LocalContentId;
            GameConfig.UiConfigChanged += this.OnUiConfigChanged;
            PluginConfiguration.WriteNewUiConfigOriginalSafe(CurrentPlayerContentId);
        }

        /// <summary>
        ///     Handles restoring base game settings on logout/game close and unsetting
        ///     the current player content id value.
        /// </summary>
        public void OnLogout(int type, int code)
        {
            RestoreAllGameSettings();
            CurrentPlayerContentId = 0;
            GameConfig.UiConfigChanged -= this.OnUiConfigChanged;
        }

        /// <summary>
        ///     Manually trigger configuration settings to reapply.
        /// </summary>
        public void OnConfigurationWindowSave() => this.UpdateGameSettingsForTerritory(ClientState.TerritoryType);

        /// <summary>
        ///     Applies specific zone overrides to settings or handles restoring them when leaving overriden zones.
        /// </summary>
        /// <param name="territoryId"></param>
        private void UpdateGameSettingsForTerritory(ushort territoryId)
        {
            var didApplyModified = false;
            var didApplyOriginal = false;
            var canApplyModified = AllowedTerritories.Any(x => territoryId == x.RowId);

            Log.Information($"Checking to overwrite or restore settings data for TerritoryId {territoryId}");

            // Modify or restore system configuration data.
            if (canApplyModified && PluginConfiguration.TerritorySystemConfiguration.TryGetValue(territoryId, out var systemConfig))
            {
                Log.Information($"TerritoryId has system setting overrides, applying modified data");
                PluginConfiguration.SystemConfigurationOverwritten = true;
                PluginConfiguration.Save();
                systemConfig.ApplyToGame();
                didApplyModified = true;
            }
            else if (PluginConfiguration.SystemConfigurationOverwritten)
            {
                Log.Information($"TerritoryId does not have any system setting overrides and they are still modified, restoring defaults");
                PluginConfiguration.OriginalSystemConfiguration.ApplyToGame();
                PluginConfiguration.SystemConfigurationOverwritten = false;
                PluginConfiguration.Save();
                didApplyOriginal = true;
            }

            // Modify or restore ui configuration data.
            if (canApplyModified && PluginConfiguration.TerritoryUiConfiguration.TryGetValue(territoryId, out var uiConfig))
            {
                Log.Information($"TerritoryId has ui settings overrides, applying modified data");
                PluginConfiguration.UiConfigurationOverwritten = true;
                PluginConfiguration.Save();
                uiConfig.ApplyToGame();
                didApplyModified = true;
            }
            else if (PluginConfiguration.UiConfigurationOverwritten && PluginConfiguration.OriginalUiConfiguration.TryGetValue(CurrentPlayerContentId, out var charConfig))
            {
                Log.Information($"TerritoryId does not have any ui settings overrides and they are still modified, restoring defaults");
                charConfig.ApplyToGame();
                PluginConfiguration.UiConfigurationOverwritten = false;
                PluginConfiguration.Save();
                didApplyOriginal = true;
            }

            // Notify depending on state changes.
            if (didApplyModified)
            {
                NotificationManager.AddNotification(new Notification()
                {
                    Title = "Configuration Modified",
                    Content = "Zone configuration applied",
                    HardExpiry = DateTime.Now.AddSeconds(5),
                    UserDismissable = false,
                    Type = NotificationType.Info
                });
            }
            else if (didApplyOriginal)

                NotificationManager.AddNotification(new Notification()
                {
                    Title = "Configuration Restored",
                    Content = "Game configuration data restored",
                    HardExpiry = DateTime.Now.AddSeconds(5),
                    UserDismissable = false,
                    Type = NotificationType.Info
                });
        }

        /// <summary>
        ///     Updates stored original system configuration when configuration is overwritten.
        /// </summary>
        private void OnSystemConfigUpdated(object? sender, ConfigChangeEvent e)
        {
            if (PluginConfiguration.SystemConfigurationOverwritten)
            {
                Log.Debug("Ignoring System Configuration update as settings have been modified by the plugin.");
                return;
            }
            PluginConfiguration.WriteNewSysConfigOriginalSafe();
            PluginConfiguration.Save();
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

            if (PluginConfiguration.UiConfigurationOverwritten)
            {
                Log.Debug("Ignoring Ui Configuration update as settings have been modified by the plugin.");
                return;
            }
            PluginConfiguration.WriteNewUiConfigOriginalSafe(CurrentPlayerContentId);
            PluginConfiguration.Save();
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
                charConfig.ApplyToGame();
                PluginConfiguration.UiConfigurationOverwritten = false;
                anyRestored = true;
                Log.Information($"Restored UiConfiguration for {CurrentPlayerContentId}");
            }
            if (PluginConfiguration.SystemConfigurationOverwritten)
            {
                PluginConfiguration.OriginalSystemConfiguration.ApplyToGame();
                PluginConfiguration.SystemConfigurationOverwritten = false;
                anyRestored = true;
                Log.Information($"Restored SystemConfiguration");
            }
            if (anyRestored)
            {
                PluginConfiguration.Save();
                NotificationManager.AddNotification(new Notification()
                {
                    Title = "Configuration Restored",
                    Content = "Game configuration data restored",
                    HardExpiry = DateTime.Now.AddSeconds(5),
                    UserDismissable = false,
                    Type = NotificationType.Info
                });
            }
        }
    }
}
