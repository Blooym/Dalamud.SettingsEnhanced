using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Game.Config;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using SettingsEnhanced.Game.Settings;
using SettingsEnhanced.Game.Settings.Attributes;
using SettingsEnhanced.Game.Settings.Interfaces;

namespace SettingsEnhanced.UI.Windows
{
    internal sealed partial class ConfigurationWindow : Window
    {
        private sealed class SelectedItem
        {
            public required ushort TerritoryId;
            public required string TerritoryName;
            public required SystemConfiguration SystemConfiguration;
            public required UiConfiguration UiConfiguration;
        }

        private static readonly Dictionary<uint, string> TerritoryList = Plugin.AllowedTerritories
            .Where(t => !string.IsNullOrEmpty(t.PlaceName.Value.Name.ExtractText())
            )
            .OrderBy(x => x.PlaceName.Value.Name.ExtractText())
            .ToDictionary(
                t => t.RowId,
                t => t.PlaceName.Value.Name.ExtractText()
            );

        private static readonly IOrderedEnumerable<IGrouping<string, PropertyInfo>> SystemConfigurationItemsGroup = typeof(SystemConfiguration)
            .GetProperties(Plugin.ConfigReflectionBindingFlags)
            .Where(p => p.GetCustomAttribute<SystemConfiguration.ConfigurationItemAttribute>() != null)
            .GroupBy(p => p.GetCustomAttribute<SystemConfiguration.ConfigurationItemAttribute>()!.InterfaceGroup)
            .OrderBy(g => g.Key);

        private static readonly IOrderedEnumerable<IGrouping<string, PropertyInfo>> UiConfigurationItemsGroup = typeof(UiConfiguration)
            .GetProperties(Plugin.ConfigReflectionBindingFlags)
            .Where(p => p.GetCustomAttribute<UiConfiguration.ConfigurationItemAttribute>() != null)
            .GroupBy(p => p.GetCustomAttribute<UiConfiguration.ConfigurationItemAttribute>()!.InterfaceGroup)
            .OrderBy(g => g.Key);

        private string searchText = "";
        private bool canSaveSettings;
        private SelectedItem? selectedItem;
        public static event Action? ConfigurationUpdated;

        public ConfigurationWindow() : base("Settings Enhanced")
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = ImGuiHelpers.ScaledVector2(750, 400),
            };
            this.Size = ImGuiHelpers.ScaledVector2(750, 400);
            this.Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
            this.SizeCondition = ImGuiCond.FirstUseEver;
            this.AllowClickthrough = false;
            this.AllowPinning = false;
            this.TitleBarButtons = [
                new()
                {
                    Icon = FontAwesomeIcon.Heart,
                    Click= (mouseButton) => Util.OpenLink("https://go.blooym.dev/donate"),
                    ShowTooltip = () => ImGui.SetTooltip("Support the developer"),
                },
                new() {
                    Icon = FontAwesomeIcon.Comment,
                    Click = (mouseButton) => Util.OpenLink("https://github.com/Blooym/Dalamud.SettingsEnhanced"),
                    ShowTooltip = () => ImGui.SetTooltip("Repository"),
                },
            ];
        }

        public override bool DrawConditions() => Plugin.ClientState.IsLoggedIn;

        public override void Draw()
        {
            if (!Plugin.PluginConfiguration.UiWarningAccepted)
            {
                DrawWarningUi();
                return;
            }
            this.DrawConfigUi();
        }

        private static void DrawWarningUi()
        {
            Plugin.GameConfig.TryGet(SystemConfigOption.FirstConfigBackup, out bool neverMadeBackup);
            ImGui.TextColored(ImGuiColors.DalamudRed, "IMPORTANT NOTICE");
            ImGui.TextWrapped("This plugin automatically modifies your game settings based on your configuration.");
            ImGui.TextWrapped("The developer has worked hard to minimize the risk of data loss due to bugs or crashes; However, you should still make regular backups using the official configuration backup tool (available in the character menu) while using this plugin.");
            ImGui.TextColored(ImGuiColors.DalamudYellow, "It's crucial that you back up your settings frequently to prevent any potential issues.");
            ImGui.TextWrapped("You will not be able to use this plugin until you've made at least one server backup. This is to ensure your settings can be restored if necessary.");
            ImGui.TextWrapped("By continuing, you acknowledge the risks and agree to regularly back up your settings as advised.");
            ImGui.NewLine();
            if (neverMadeBackup)
            {
                ImGui.TextColored(ImGuiColors.DalamudRed, "You must create a server backup before proceeding.");
                return;
            }

            ImGui.BeginDisabled(neverMadeBackup || !ImGui.IsKeyDown(ImGuiKey.LeftShift));
            if (ImGui.Button("I acknowledge the risks and wish to continue"))
            {
                Plugin.PluginConfiguration.UiWarningAccepted = true;
                Plugin.PluginConfiguration.Save();
            }
            ImGui.EndDisabled();
            ImGuiComponents.HelpMarker("Hold 'Left Shift' to activate the button");
        }

        private void DrawConfigUi()
        {
            if (ImGui.BeginChild("UiWithSidebarChild", new(default, ImGui.GetContentRegionAvail().Y - (20 * ImGuiHelpers.GlobalScale)), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.BeginTable("UiWithSidebarTable", 2))
                {
                    ImGui.TableSetupColumn("Sidebar", ImGuiTableColumnFlags.WidthFixed, ImGui.GetContentRegionAvail().X * 0.28f);
                    ImGui.TableSetupColumn("Main", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    if (ImGui.BeginChild("SidebarChild", default, true))
                    {
                        this.DrawSidebar();
                    }
                    ImGui.EndChild();
                    ImGui.TableNextColumn();
                    if (ImGui.BeginChild("MainContent", default, true))
                    {
                        this.DrawMainContent();
                    }
                    ImGui.EndChild();
                    ImGui.EndTable();
                }
            }
            ImGui.EndChild();
            if (Plugin.PluginConfiguration.UiConfigurationOverwritten || Plugin.PluginConfiguration.SystemConfigurationOverwritten)
            {
                ImGuiHelpers.SafeTextColoredWrapped(ImGuiColors.DalamudYellow, $"Currently using zone settings");
                ImGuiComponents.HelpMarker("Changing a setting you have overwritten in this zone via the in-game options menu will only apply temporarily and will not be saved.\n\nChanging any setting you have not overwritten will apply to your global game configuration as normal.", FontAwesomeIcon.QuestionCircle);
            }
            else
            {
                ImGuiHelpers.SafeTextColoredWrapped(ImGuiColors.HealerGreen, "Currently using game settings");
                ImGuiComponents.HelpMarker("All changes to your settings will work as normal.", FontAwesomeIcon.QuestionCircle);
            }
        }

        private void DrawSidebar()
        {
            var filtered = TerritoryList
                                .Where(x => x.Value.Contains(this.searchText, StringComparison.InvariantCultureIgnoreCase));
            var grouped = filtered
                .GroupBy(x => Plugin.PluginConfiguration.TerritorySystemConfiguration.ContainsKey((ushort)x.Key)
                        || Plugin.PluginConfiguration.TerritoryUiConfiguration.ContainsKey((ushort)x.Key))
                .OrderByDescending(g => g.Key);

            if (ImGui.BeginChild("SearchbarChild", new(0, (25 * ImGuiHelpers.GlobalScale) - ImGui.GetContentRegionAvail().Y)))
            {
                ImGui.SetNextItemWidth(-1);
                ImGui.InputTextWithHint("##Searchbar", "Search...", ref this.searchText, 50);
            }
            ImGui.EndChild();
            ImGui.Separator();

            if (ImGui.BeginChild("SearchResultsChild"))
            {
                foreach (var group in grouped)
                {
                    var hasSettings = group.Key;
                    ImGui.TextDisabled(hasSettings ? "Custom Settings" : "Default Settings");
                    foreach (var (id, name) in group)
                    {
                        var currentTerritory = Plugin.ClientState.TerritoryType == id;
                        if (currentTerritory)
                            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudViolet);
                        if (ImGui.Selectable($"{name}##{id}", id == this.selectedItem?.TerritoryId))
                        {
                            this.selectedItem = new()
                            {
                                TerritoryId = (ushort)id,
                                TerritoryName = name,
                                SystemConfiguration = Plugin.PluginConfiguration.TerritorySystemConfiguration
                                .GetValueOrDefault(
                                    (ushort)id,
                                    ((SystemConfiguration)Plugin.PluginConfiguration.OriginalSystemConfiguration.Clone()).DepersistAllProperties()

                                ),
                                UiConfiguration = Plugin.PluginConfiguration.TerritoryUiConfiguration.GetValueOrDefault(
                                    (ushort)id,
                                    ((UiConfiguration)Plugin.PluginConfiguration.OriginalUiConfiguration[Plugin.CurrentPlayerContentId].Clone()).DepersistAllProperties()
                                )
                            };
                        }
                        if (currentTerritory)
                            ImGui.PopStyleColor();
                    }
                    ImGuiHelpers.ScaledDummy(8);
                }
            }
            ImGui.EndChild();
        }

        private void DrawMainContent()
        {
            if (this.selectedItem is not null)
            {
                ImGui.TextDisabled(this.selectedItem.TerritoryName);
                ImGui.Separator();
                if (ImGui.BeginChild("ConfigurationContent", new(default, ImGui.GetContentRegionAvail().Y - (26 * ImGuiHelpers.GlobalScale))))
                {
                    if (ImGui.BeginTabBar("ConfigTabs"))
                    {
                        if (ImGui.BeginTabItem("System Configuration"))
                        {
                            if (ImGui.BeginChild("SystemConfChild"))
                            {
                                foreach (var group in SystemConfigurationItemsGroup)
                                {
                                    if (ImGui.CollapsingHeader(group.Key.ToString()))
                                    {
                                        this.DrawConfigurationGroup<SystemConfiguration, SystemConfiguration.ConfigurationItemAttribute>(group, this.selectedItem.SystemConfiguration);
                                    }
                                }
                            }
                            ImGui.EndChild();
                            ImGui.EndTabItem();
                        }

                        if (ImGui.BeginTabItem("Character Configuration"))
                        {
                            if (ImGui.BeginChild("CharConfigChild"))
                            {
                                foreach (var group in UiConfigurationItemsGroup)
                                {
                                    if (ImGui.CollapsingHeader(group.Key.ToString()))
                                    {
                                        this.DrawConfigurationGroup<UiConfiguration, UiConfiguration.ConfigurationItemAttribute>(group, this.selectedItem.UiConfiguration);
                                    }
                                }
                            }
                            ImGui.EndChild();
                            ImGui.EndTabItem();
                        }
                    }
                    ImGui.EndTabBar();
                }
                ImGui.EndChild();
                ImGui.Separator();

                ImGui.BeginDisabled(!this.canSaveSettings);
                if (ImGui.Button("Apply Settings"))
                {
                    // Set or remove configurations depending on if they have any persists left.
                    if (this.selectedItem.SystemConfiguration.AnyPersistedProperties())
                        Plugin.PluginConfiguration.TerritorySystemConfiguration[this.selectedItem.TerritoryId] = this.selectedItem.SystemConfiguration;
                    else
                        Plugin.PluginConfiguration.TerritorySystemConfiguration.Remove(this.selectedItem.TerritoryId);
                    if (this.selectedItem.UiConfiguration.AnyPersistedProperties())
                        Plugin.PluginConfiguration.TerritoryUiConfiguration[this.selectedItem.TerritoryId] = this.selectedItem.UiConfiguration;
                    else
                        Plugin.PluginConfiguration.TerritoryUiConfiguration.Remove(this.selectedItem.TerritoryId);
                    Plugin.PluginConfiguration.Save();
                    ConfigurationUpdated?.Invoke();
                    this.canSaveSettings = false;
                }
                ImGui.EndDisabled();
                ImGui.SameLine();
                ImGui.BeginDisabled(!ImGui.IsKeyDown(ImGuiKey.LeftShift));
                if (ImGui.Button("Delete Settings"))
                {
                    this.canSaveSettings = false;
                    var configsDeleted = false;
                    if (Plugin.PluginConfiguration.TerritorySystemConfiguration.Remove(this.selectedItem.TerritoryId))
                    {
                        this.selectedItem.SystemConfiguration = ((SystemConfiguration)Plugin.PluginConfiguration.OriginalSystemConfiguration.Clone()).DepersistAllProperties();
                        configsDeleted = true;
                    }
                    if (Plugin.PluginConfiguration.TerritoryUiConfiguration.Remove(this.selectedItem.TerritoryId))
                    {
                        this.selectedItem.UiConfiguration = ((UiConfiguration)Plugin.PluginConfiguration.OriginalUiConfiguration[Plugin.ClientState.LocalContentId].Clone()).DepersistAllProperties();
                        configsDeleted = true;
                    }
                    if (configsDeleted)
                    {
                        Plugin.PluginConfiguration.Save();
                        ConfigurationUpdated?.Invoke();
                    }
                }
                ImGui.EndDisabled();
                ImGuiComponents.HelpMarker("Hold 'Left Shift' to enable deletion. Your original settings will be automatically reapplied.");
            }
        }

        private void DrawConfigurationGroup<TConfig, TAttribute>(IGrouping<string, PropertyInfo> group, TConfig configuration)
            where TConfig : IGameConfiguration<TConfig>
            where TAttribute : Attribute, IUiDisplay
        {
            var subGroups = group
                .GroupBy(p => p.GetCustomAttribute<TAttribute>()!.InterfaceHeaderName)
                .OrderBy(g => g.Key);
            foreach (var subGroup in subGroups)
            {
                if (!string.IsNullOrWhiteSpace(subGroup.Key))
                    ImGui.TextDisabled(subGroup.Key.ToString());

                foreach (var prop in subGroup)
                {
                    this.DrawConfigurationProperty<TConfig, TAttribute>(prop, configuration);
                }
                ImGuiHelpers.ScaledDummy(6);
            }
        }

        private void DrawConfigurationProperty<TConfig, TAttribute>(PropertyInfo prop, TConfig configuration)
            where TConfig : IGameConfiguration<TConfig>
            where TAttribute : Attribute, IUiDisplay
        {
            var configOptionAttribute = prop.GetCustomAttribute<TAttribute>();
            var displayName = configOptionAttribute?.InterfaceName ?? prop.Name;

            if (configOptionAttribute?.Indented is true)
            {
                ImGui.Indent();
            }
            this.DrawPropertyResetButton(configuration, prop);
            ImGui.SameLine();

            if (prop.PropertyType.IsEnum)
                this.DrawEnumProperty(configuration, prop, displayName);
            else if (prop.PropertyType == typeof(uint))
                this.DrawUintProperty(configuration, prop, displayName);
            else if (prop.PropertyType == typeof(bool))
                this.DrawBoolProperty(configuration, prop, displayName);
            else if (prop.PropertyType == typeof(string))
                this.DrawStringProperty(configuration, prop, displayName);

            if (configOptionAttribute?.Indented is true)
            {
                ImGui.Unindent();
            }
        }

        private void DrawPropertyResetButton<T>(T configuration, PropertyInfo prop) where T : IGameConfiguration<T>
        {
            ImGui.BeginDisabled(!configuration.IsPropertyPersisted(prop));
            if (ImGuiComponents.IconButton(prop.Name, FontAwesomeIcon.Sync))
            {
                if (typeof(T) == typeof(UiConfiguration))
                {
                    if (Plugin.PluginConfiguration.OriginalUiConfiguration.TryGetValue(Plugin.CurrentPlayerContentId, out var uiConfig))
                    {
                        var propertyValue = typeof(UiConfiguration).GetProperty(prop.Name)?.GetValue(uiConfig);
                        if (propertyValue != null)
                        {
                            configuration.SetPropertyValue(prop, propertyValue);
                            configuration.DepersistProperty(prop);
                            this.canSaveSettings = true;
                        }
                        else
                        {
                            Plugin.Log.Error($"Failed to reset setting: {prop.Name} does not have value on original ui configuration for current character");
                        }
                    }
                }
                else if (typeof(T) == typeof(SystemConfiguration))
                {
                    var defaultValue = typeof(SystemConfiguration).GetProperty(prop.Name)?.GetValue(Plugin.PluginConfiguration.OriginalSystemConfiguration);
                    if (defaultValue is not null)
                    {
                        configuration.SetPropertyValue(prop, defaultValue);
                        configuration.DepersistProperty(prop);
                        this.canSaveSettings = true;
                    }
                    else
                    {
                        Plugin.Log.Error($"Failed to reset system setting: {prop.Name} does not have value on original system configuration");
                    }
                }
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Reset this option to its original value");
            }
            ImGui.EndDisabled();
        }

        private void DrawEnumProperty<T>(T configuration, PropertyInfo prop, string displayName) where T : IGameConfiguration<T>
        {
            var enumValues = Enum.GetValues(prop.PropertyType).Cast<Enum>().ToArray();
            var value = prop.GetValue(configuration) as Enum;
            if (ImGui.BeginCombo(displayName, value?.ToString() ?? "None"))
            {
                foreach (var enumValue in enumValues)
                {
                    var isSelected = enumValue.Equals(value);
                    if (ImGui.Selectable(enumValue.ToString(), isSelected))
                    {
                        configuration.SetPropertyValue(prop, Enum.ToObject(prop.PropertyType, enumValue));
                        configuration.PersistProperty(prop);
                        this.canSaveSettings = true;
                    }
                }
                ImGui.EndCombo();
            }
        }

        private void DrawStringProperty<T>(T configuration, PropertyInfo prop, string displayName) where T : IGameConfiguration<T>
        {
            var range = prop.GetCustomAttribute<ConfigurationInputRangeAttribute>() ?? new(0, 100);
            var currentValue = (string)(prop.GetValue(configuration) ?? "");
            if (ImGui.InputText(displayName, ref currentValue, (uint)range.Max) && currentValue.Length > range.Min)
            {
                configuration.SetPropertyValue(prop, currentValue);
                configuration.PersistProperty(prop);
                this.canSaveSettings = true;
            }
        }

        private void DrawUintProperty<T>(T configuration, PropertyInfo prop, string displayName) where T : IGameConfiguration<T>
        {
            var range = prop.GetCustomAttribute<ConfigurationInputRangeAttribute>() ?? new(0, 100);
            var currentValue = (uint)(prop.GetValue(configuration) ?? 0);
            var refValue = (int)currentValue;
            if (ImGui.SliderInt(displayName, ref refValue, range.Min, range.Max, default, ImGuiSliderFlags.AlwaysClamp))
            {
                configuration.SetPropertyValue(prop, (uint)refValue);
                configuration.PersistProperty(prop);
                this.canSaveSettings = true;
            }
        }

        private void DrawBoolProperty<T>(T configuration, PropertyInfo prop, string displayName) where T : IGameConfiguration<T>
        {
            var currentValue = (bool)(prop.GetValue(configuration) ?? false);
            if (ImGui.Checkbox(displayName, ref currentValue))
            {
                configuration.SetPropertyValue(prop, currentValue);
                configuration.PersistProperty(prop);
                this.canSaveSettings = true;
            }
        }
    }
}
