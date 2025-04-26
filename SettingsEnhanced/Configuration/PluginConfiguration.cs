using System.Collections.Generic;
using Dalamud.Configuration;
using SettingsEnhanced.Game.Settings;

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
        ///     Whether or not the first time run warning has been read.
        ///     No user interface operations can be done until this is true.
        /// </summary>
        public bool UiWarningAccepted;

        /// <summary>
        ///     Whether or not the System Configuration in the current game state were modified by the plugin.
        /// </summary>
        /// <remarks>
        ///     If a user disables this plugin original settings are restored, this is set to true before shutdown.
        ///     If a user is not in an overriden zone and the plugin has restored original settings, this is true.
        ///     If a user is in an overriden zone or the plugin crashes without safely restoring settings, this is false.
        /// </remarks>
        public bool SystemConfigurationOverwritten;

        /// <summary>
        ///     Whether or not the Ui Configuration in the current game state were modified by the plugin.
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
        public SystemConfiguration OriginalSystemConfiguration = SystemConfiguration.FromGame().PersistAllValues();

        /// <summary>
        ///     A snapshot of the game's Ui Configuration without plugin modification.
        /// </summary>
        public Dictionary<ulong, UiConfiguration> OriginalUiConfiguration = [];

        /// <summary>
        ///     Per-territory System Configuration.
        /// </summary>
        public Dictionary<ushort, SystemConfiguration> TerritorySystemConfiguration = [];

        /// <summary>
        ///     Per-territory Ui Configuration.
        /// </summary>
        public Dictionary<ushort, UiConfiguration> TerritoryUiConfiguration = [];

        /// <summary>
        ///     Save the current value of the plugin configuration.
        /// </summary>
        public void Save() => Plugin.PluginInterface.SavePluginConfig(this);

        /// <summary>
        ///     Load the plugin configuration from disk.
        /// </summary>
        /// <returns></returns>
        public static PluginConfiguration Load() => Plugin.PluginInterface.GetPluginConfig() as PluginConfiguration ?? new();

        public void WriteNewSysConfigOriginalSafe()
        {
            if (this.SystemConfigurationOverwritten)
            {
                Plugin.Log.Debug("Ignoring WriteNewSysConfig call because it is not currently safe to do so");
                return;
            }

            Plugin.Log.Debug($"SystemConfiguration not overwritten - setting current game values as OriginalSystemConfiguration");
            this.OriginalSystemConfiguration = SystemConfiguration.FromGame().PersistAllValues();
            this.Save();
        }

        public void WriteNewUiConfigOriginalSafe(ulong playerContentId)
        {
            if (playerContentId == 0 || this.UiConfigurationOverwritten)
            {
                Plugin.Log.Debug("Ignoring WriteNewUiConfig call because it is not currently safe to do so");
                return;
            }

            Plugin.Log.Debug($"UIConfiguration not overwritten - writing current game values as OriginalUiConfiguration for player {playerContentId}");
            this.OriginalUiConfiguration[playerContentId] = UiConfiguration.FromGame().PersistAllValues();
            this.Save();
        }
    }
}
