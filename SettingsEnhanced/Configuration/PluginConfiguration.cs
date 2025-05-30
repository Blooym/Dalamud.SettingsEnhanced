using System.Collections.Generic;
using Dalamud.Configuration;
using SettingsEnhanced.Game.Settings;
using SystemConfiguration = SettingsEnhanced.Game.Settings.SystemConfiguration;

namespace SettingsEnhanced.Configuration
{
    /// <summary>
    ///     Provides access to and determines the plugin configuration.
    /// </summary>
    internal sealed class PluginConfiguration : IPluginConfiguration
    {
        /// <summary>
        ///     The current configuration version, incremented on breaking changes.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        ///     Whether the first time run warning has been read.
        ///     No user interface operations can be done until this is true.
        /// </summary>
        public bool UiWarningAccepted;

        /// <summary>
        ///     Whether the System Configuration in the current game state were modified by the plugin.
        /// </summary>
        /// <remarks>
        ///     If a user disables this plugin original settings are restored, this is set to true before shutdown.
        ///     If a user is not in an overriden zone and the plugin has restored original settings, this is true.
        ///     If a user is in an overriden zone or the plugin crashes without safely restoring settings, this is false.
        /// </remarks>
        public bool SystemConfigurationOverwritten;

        /// <summary>
        ///     Whether the Ui Configuration in the current game state were modified by the plugin.
        /// </summary>
        /// <remarks>
        ///     If a user disables this plugin original settings are restored, this is set to true before shutdown.
        ///     If a user is not in an overriden zone and the plugin has restored original settings, this is true.
        ///     If a user is in an overriden zone or the plugin crashes without safely restoring settings, this is false.
        /// </remarks>
        public bool UiConfigurationOverwritten;

        /// <summary>
        ///     A snapshot of the game's System Configuration without plugin modification.
        /// </summary>
        public SystemConfiguration OriginalSystemConfiguration = SystemConfiguration.FromGame().PersistAllProperties();

        /// <summary>
        ///     A snapshot of the game's Ui Configuration without plugin modification.
        /// </summary>
        public readonly Dictionary<ulong, UiConfiguration> OriginalUiConfiguration = [];

        /// <summary>
        ///     Per-territory System Configuration.
        /// </summary>
        public readonly Dictionary<ushort, SystemConfiguration> TerritorySystemConfiguration = [];

        /// <summary>
        ///     Per-territory Ui Configuration.
        /// </summary>
        public readonly Dictionary<ushort, UiConfiguration> TerritoryUiConfiguration = [];

        /// <summary>
        ///     Save the current value of the plugin configuration.
        /// </summary>
        public void Save() => Plugin.PluginInterface.SavePluginConfig(this);

        /// <summary>
        ///     Load the plugin configuration from disk.
        /// </summary>
        /// <returns></returns>
        public static PluginConfiguration Load() => Plugin.PluginInterface.GetPluginConfig() as PluginConfiguration ?? new();

        /// <summary>
        ///     Write a new system configuration original value
        /// </summary>
        /// <remarks>
        ///     This method will only update the system configuration if it is safe to do so.
        /// </remarks>
        public bool WriteNewSysConfigOriginalSafe()
        {
            if (this.SystemConfigurationOverwritten)
            {
                Plugin.Log.Debug("Ignoring WriteNewSysConfigOriginalSafe call because it is not currently safe to do so");
                return false;
            }
            Plugin.Log.Debug($"SystemConfiguration not overwritten - setting current game values as OriginalSystemConfiguration");
            this.OriginalSystemConfiguration = SystemConfiguration.FromGame().PersistAllProperties();
            this.Save();
            return true;
        }

        /// <summary>
        ///     Write a new ui configuration original value
        /// </summary>
        /// <remarks>
        ///     This method will only update the ui configuration if it is safe to do so.
        /// </remarks>
        public bool WriteNewUiConfigOriginalSafe(ulong playerContentId)
        {
            if (playerContentId is 0 || this.UiConfigurationOverwritten)
            {
                Plugin.Log.Debug("Ignoring WriteNewUiConfigOriginalSafe call because it is not currently safe to do so");
                return false;
            }
            Plugin.Log.Debug($"UIConfiguration not overwritten - writing current game values as OriginalUiConfiguration for player {playerContentId}");
            this.OriginalUiConfiguration[playerContentId] = UiConfiguration.FromGame().PersistAllProperties();
            this.Save();
            return true;
        }
    }
}
