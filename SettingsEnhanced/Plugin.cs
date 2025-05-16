using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Dalamud.Game.Config;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using SettingsEnhanced.Configuration;
using SettingsEnhanced.Game.Enums;
using SettingsEnhanced.Game.Settings;
using SettingsEnhanced.Game.Settings.Attributes;
using SettingsEnhanced.Game.Settings.Util;
using SettingsEnhanced.Resources.Localization;
using SettingsEnhanced.UI;
using SettingsEnhanced.UI.Windows;
using LuminaSheets = Lumina.Excel.Sheets;
using SystemConfiguration = SettingsEnhanced.Game.Settings.SystemConfiguration;

namespace SettingsEnhanced
{
    internal sealed class Plugin : IDalamudPlugin
    {
        private enum ConfigApplyType
        {
            None = 0,
            Original = 1,
            Modified = 2,
        }

        // Safety: valid from plugin start.
#pragma warning disable CS8618
        [PluginService] private static IDataManager DataManager { get; set; }
        [PluginService] private static INotificationManager NotificationManager { get; set; }
        [PluginService] private static IFramework Framework { get; set; }
        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; }
        [PluginService] public static IClientState ClientState { get; private set; }
        [PluginService] public static IPluginLog Log { get; private set; }
        [PluginService] public static IGameConfig GameConfig { get; private set; }
        private static WindowManager WindowManager { get; set; }
        private static LocalizationManager LocalizationManager { get; set; }
        public static PluginConfiguration PluginConfiguration { get; private set; }
        public static ImmutableArray<LuminaSheets.TerritoryType> EnabledTerritories;
        public static ExcelSheet<LuminaSheets.Addon> AddonSheet;
#pragma warning restore CS8618

        /// <summary>
        ///     How long config change notifications will be shown to the user in seconds.
        /// </summary>
        private const uint ConfigChangeNotificationShowDurationSecs = 4;

        /// <summary>
        ///     Binding flags to use when doing reflection on configuration classes.
        /// </summary>
        public const BindingFlags ConfigReflectionBindingFlags = BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        ///     Territory use types that are enabled for use with this plugin.
        /// </summary>
        private static readonly uint[] EnabledTerritoryUse = [
            (uint)TerritoryIntendedUse.Town,
            (uint)TerritoryIntendedUse.OpenWorld,
            (uint)TerritoryIntendedUse.Inn,
            (uint)TerritoryIntendedUse.Dungeon,
            (uint)TerritoryIntendedUse.VariantDungeon,
            (uint)TerritoryIntendedUse.AllianceRaid,
            (uint)TerritoryIntendedUse.Trial,
            (uint)TerritoryIntendedUse.HousingArea,
            (uint)TerritoryIntendedUse.Raids,
            (uint)TerritoryIntendedUse.RaidFights,
            (uint)TerritoryIntendedUse.GoldSaucer,
            (uint)TerritoryIntendedUse.PalaceOfTheDead,
            (uint)TerritoryIntendedUse.FreeCompanyGarrison,
            (uint)TerritoryIntendedUse.TreasureMapInstance,
            (uint)TerritoryIntendedUse.Eureka,
            (uint)TerritoryIntendedUse.Bozja,
            (uint)TerritoryIntendedUse.IslandSanctuary,
            (uint)TerritoryIntendedUse.CriterionDungeon,
            (uint)TerritoryIntendedUse.CriterionDungeonSavage,
            (uint)TerritoryIntendedUse.LeapOfFaith,
            (uint)TerritoryIntendedUse.MaskedCarnival,
            (uint)TerritoryIntendedUse.CosmicExploration
        ];

        /// <summary>
        ///     Territories that cannot be used with this plugin regardless of other conditions.
        /// </summary>
        private static readonly uint[] TerritoryIdBlocklist = [];
        /// <summary>
        ///     The local player's current territory.
        /// </summary>
        private static ushort CurrentPlayerTerritoryId { get; set; }

        /// <summary>
        ///     The local player's content id.
        /// </summary>
        public static ulong CurrentPlayerContentId { get; private set; }

        public Plugin()
        {
            AddonSheet = DataManager.Excel.GetSheet<LuminaSheets.Addon>();
            EnabledTerritories = [.. DataManager.Excel.GetSheet<LuminaSheets.TerritoryType>().Where(x => !TerritoryIdBlocklist.Contains(x.RowId) && EnabledTerritoryUse.Contains(x.TerritoryIntendedUse.RowId))];
            LocalizationManager = new();
            PluginConfiguration = PluginConfiguration.Load();
            WindowManager = new();

            // Warn about being left in a bad state
            if (PluginConfiguration.SystemConfigurationOverwritten || PluginConfiguration.UiConfigurationOverwritten)
            {
                Log.Warning($"Configuration data was not set back to default last plugin shutdown, possible game crash? SystemOverwritten: {PluginConfiguration.SystemConfigurationOverwritten} UIOverwritten: {PluginConfiguration.UiConfigurationOverwritten}");
                NotificationManager.AddNotification(new()
                {
                    Title = Strings.Notification_BadConfigurationState_Title,
                    Content = Strings.Notification_BadConfigurationState_Content,
                    Type = NotificationType.Warning,
                    Minimized = false,
                    InitialDuration = TimeSpan.FromSeconds(15),
                    ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(5),
                    RespectUiHidden = false,
                });
            }

            Framework.Update += this.OnFrameworkUpdate;
            GameConfig.SystemChanged += this.OnSystemConfigUpdated;
            ConfigurationWindow.ConfigurationUpdated += this.OnConfigurationWindowSave;
            PluginConfiguration.WriteNewSysConfigOriginalSafe();
        }

        public void Dispose()
        {
            Framework.Update -= this.OnFrameworkUpdate;
            GameConfig.SystemChanged -= this.OnSystemConfigUpdated;
            GameConfig.UiConfigChanged -= this.OnUiConfigChanged;
            GameConfig.UiControlChanged -= this.OnUiControlChanged;
            ConfigurationWindow.ConfigurationUpdated -= this.OnConfigurationWindowSave;
            WindowManager.Dispose();
            RestoreAllGameSettings();   // Apply the base game settings again.
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
        private void OnLogout()
        {
            RestoreAllGameSettings();
            CurrentPlayerContentId = 0;
            GameConfig.UiControlChanged -= this.OnUiControlChanged;
            GameConfig.UiConfigChanged -= this.OnUiConfigChanged;
        }

        public void OnFrameworkUpdate(IFramework _)
        {
            // Handle login and logout.
            var contentId = ClientState.LocalContentId;
            if (contentId != CurrentPlayerContentId)
            {
                if (contentId is 0)
                {
                    this.OnLogout();
                }
                else
                {
                    this.OnLogin();
                }
            }

            // Handle territory change.
            var territoryId = ClientState.TerritoryType;
            if (territoryId is not 0 && territoryId != CurrentPlayerTerritoryId)
            {
                CurrentPlayerTerritoryId = territoryId;
                TriggerConfigUpdateWithTerritory(territoryId);
            }
        }

        private void OnConfigurationWindowSave() => TriggerConfigUpdateWithTerritory(ClientState.TerritoryType);

        /// <summary>
        ///     Updates stored original system configuration when configuration is overwritten.
        /// </summary>
        private void OnSystemConfigUpdated(object? sender, ConfigChangeEvent e)
        {
            // Safety: only handles config options the plugin handles.
            var option = (SystemConfigOption)e.Option;
            var updatedOptionProperty = typeof(SystemConfiguration)
                .GetProperties(ConfigReflectionBindingFlags)
                .FirstOrDefault(x => x.GetCustomAttribute<SystemConfigurationItemAttribute>()?.ConfigOption == option);
            if (updatedOptionProperty is null)
            {
                return;
            }

            // Safety: prevent writes while plugin has applied overwrites.
            if (PluginConfiguration.SystemConfigurationOverwritten)
            {
                Log.Debug($"SystemConfiguration not saving settings update as they are overwritten by the plugin");
                return;
            }
            // Safety: only apply valid game data
            else if (GameConfigUtil.TryGetGameConfigValue(option, updatedOptionProperty.PropertyType, out var value))
            {
                Log.Debug($"SystemConfiguration saving {option} to original value");
                PluginConfiguration.OriginalSystemConfiguration.SetPropertyValue(updatedOptionProperty, value);
                PluginConfiguration.Save();
            }
        }

        /// <summary>
        ///     Updates stored original ui configuration when configuration is overwritten.
        /// </summary>
        private void OnUiConfigChanged(object? _, ConfigChangeEvent e)
        {
            if (CurrentPlayerContentId is 0)
            {
                return;
            }

            // Safety: only handles config options the plugin handles.
            var option = (UiConfigOption)e.Option;
            var updatedOptionProperty = typeof(UiConfiguration)
                .GetProperties(ConfigReflectionBindingFlags)
                .FirstOrDefault(x => x.GetCustomAttribute<UiConfigurationItemAttribute>()?.ConfigOption == option);
            if (updatedOptionProperty is null)
            {
                return;
            }

            // Safety: prevent writes while plugin has applied overwrites.
            if (PluginConfiguration.UiConfigurationOverwritten)
            {
                Log.Debug($"UiConfiguration not saving settings update as they are overwritten by the plugin");
                return;
            }
            // Safety: only apply valid game data
            else if (GameConfigUtil.TryGetGameConfigValue(option, updatedOptionProperty.PropertyType, out var value))
            {
                Log.Debug($"UiConfiguration saving {option} to original value");
                PluginConfiguration.OriginalUiConfiguration[CurrentPlayerContentId].SetPropertyValue(updatedOptionProperty, value);
                PluginConfiguration.Save();
            }
        }

        /// <summary>
        ///     Updates stored original ui configuration when configuration is overwritten.
        /// </summary>
        private void OnUiControlChanged(object? _, ConfigChangeEvent e)
        {
            if (CurrentPlayerContentId is 0)
            {
                return;
            }

            // Safety: only handles config options the plugin handles.
            var option = (UiControlOption)e.Option;
            var updatedOptionProperty = typeof(UiConfiguration)
                .GetProperties(ConfigReflectionBindingFlags)
                .FirstOrDefault(x => x.GetCustomAttribute<UiConfigurationItemAttribute>()?.ControlOption == option);
            if (updatedOptionProperty is null)
            {
                return;
            }

            // Safety: prevent writes while plugin has applied overwrites.
            if (PluginConfiguration.UiConfigurationOverwritten)
            {
                Log.Debug($"UiConfiguration not saving settings update as they are overwritten by the plugin");
                return;
            }
            // Safety: only apply valid game data
            else if (GameConfigUtil.TryGetGameConfigValue(option, updatedOptionProperty.PropertyType, out var value))
            {
                Log.Debug($"UiConfiguration saving {option} to original value");
                PluginConfiguration.OriginalUiConfiguration[CurrentPlayerContentId].SetPropertyValue(updatedOptionProperty, value);
                PluginConfiguration.Save();
            }
        }

        private static void TriggerConfigUpdateWithTerritory(ushort territoryId)
        {
            Log.Debug($"Checking if plugin should overwrite or restore game settings data for {territoryId}");
            var applyType = ConfigApplyType.None;

            // Explicitly remove configurations for territories that aren't allowed.
            if (
                EnabledTerritories.All(x => territoryId != x.RowId) &&
                (
                    PluginConfiguration.TerritorySystemConfiguration.ContainsKey(territoryId) ||
                    PluginConfiguration.TerritoryUiConfiguration.ContainsKey(territoryId)
                )
            )
            {
                Log.Warning($"Configuration contains a territory override for {territoryId} which isn't in the allowlist, it will be removed and this event will be ignored.");
                PluginConfiguration.TerritorySystemConfiguration.Remove(territoryId);
                PluginConfiguration.TerritoryUiConfiguration.Remove(territoryId);
                PluginConfiguration.Save();
                return;
            }

            // Ignore PvP entirely.
            if (ClientState.IsPvPExcludingDen)
            {
                Log.Warning("Ignoring territory override as the client is currently in PvP.");
                return;
            }

            // Modify or restore system configuration data.
            if (PluginConfiguration.TerritorySystemConfiguration.TryGetValue(territoryId, out var systemConfig))
            {
                Log.Information($"{territoryId} has system setting overrides, applying modified data");

                // Safety: Apply base game configuaration before to prevent
                // multiple overwrites overlapping.
                if (PluginConfiguration.SystemConfigurationOverwritten)
                {
                    PluginConfiguration.OriginalSystemConfiguration.ApplyToGame();
                }

                // Safety: Must be marked as overwritten before applied.
                PluginConfiguration.SystemConfigurationOverwritten = true;
                PluginConfiguration.Save();
                systemConfig.ApplyToGame();
                applyType = ConfigApplyType.Modified;
            }
            else if (PluginConfiguration.SystemConfigurationOverwritten)
            {
                Log.Information($"{territoryId} does not have any system setting overrides and they are still modified, restoring game defaults");
                // Safety: Must be restored befored unmarked as overwritten.
                PluginConfiguration.OriginalSystemConfiguration.ApplyToGame();
                PluginConfiguration.SystemConfigurationOverwritten = false;
                PluginConfiguration.Save();
                applyType = ConfigApplyType.Original;
            }

            // Modify or restore ui configuration data.
            PluginConfiguration.OriginalUiConfiguration.TryGetValue(CurrentPlayerContentId, out var originalUiConfig);
            if (PluginConfiguration.TerritoryUiConfiguration.TryGetValue(territoryId, out var uiConfig))
            {
                Log.Information($"{territoryId} has ui settings overrides, applying modified data");

                // Safety: Apply base game configuaration before to prevent
                // multiple overwrites overlapping.
                if (PluginConfiguration.UiConfigurationOverwritten && originalUiConfig is not null)
                {
                    originalUiConfig.ApplyToGame();
                }

                // Safety: Must be marked as overwritten before applied.
                PluginConfiguration.UiConfigurationOverwritten = true;
                PluginConfiguration.Save();
                uiConfig.ApplyToGame();
                applyType = ConfigApplyType.Modified;
            }
            else if (PluginConfiguration.UiConfigurationOverwritten && originalUiConfig is not null)
            {
                Log.Information($"{territoryId} does not have any ui settings overrides and they are still modified, restoring game defaults");
                // Safety: Must be restored befored unmarked as overwritten.
                originalUiConfig.ApplyToGame();
                PluginConfiguration.UiConfigurationOverwritten = false;
                PluginConfiguration.Save();
                applyType = ConfigApplyType.Original;
            }

            // Notify depending on apply type.
            switch (applyType)
            {
                case ConfigApplyType.None:
                    break;
                case ConfigApplyType.Original:
                    NotificationManager.AddNotification(new Notification()
                    {
                        Title = Strings.Notification_ConfigurationRestored_Title,
                        Content = Strings.Notification_ConfigurationRestored_Content,
                        HardExpiry = DateTime.Now.AddSeconds(ConfigChangeNotificationShowDurationSecs),
                        Type = NotificationType.Info
                    });
                    break;
                case ConfigApplyType.Modified:
                    NotificationManager.AddNotification(new Notification()
                    {
                        Title = Strings.Notification_ConfigurationModified_Title,
                        Content = Strings.Notification_ConfigurationModified_Content,
                        HardExpiry = DateTime.Now.AddSeconds(ConfigChangeNotificationShowDurationSecs),
                        Type = NotificationType.Info
                    });
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     Restores all game configuration values to their original settings.
        /// </summary>
        private static void RestoreAllGameSettings()
        {
            Log.Debug($"Checking if restoring all game settings to original values is needed");
            var hasRestored = false;
            if (PluginConfiguration.UiConfigurationOverwritten && PluginConfiguration.OriginalUiConfiguration.TryGetValue(CurrentPlayerContentId, out var charConfig))
            {
                charConfig.ApplyToGame();
                PluginConfiguration.UiConfigurationOverwritten = false;
                hasRestored = true;
                Log.Information($"Restored UiConfiguration for {CurrentPlayerContentId}");
            }
            if (PluginConfiguration.SystemConfigurationOverwritten)
            {
                PluginConfiguration.OriginalSystemConfiguration.ApplyToGame();
                PluginConfiguration.SystemConfigurationOverwritten = false;
                hasRestored = true;
                Log.Information($"Restored SystemConfiguration");
            }

            if (hasRestored)
            {
                PluginConfiguration.Save();
                NotificationManager.AddNotification(new Notification()
                {
                    Title = Strings.Notification_ConfigurationRestored_Title,
                    Content = Strings.Notification_ConfigurationRestored_Content,
                    HardExpiry = DateTime.Now.AddSeconds(ConfigChangeNotificationShowDurationSecs),
                    Type = NotificationType.Info
                });
            }
        }
    }
}
