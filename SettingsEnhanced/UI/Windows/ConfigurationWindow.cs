using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        private static readonly Dictionary<uint, string> TerritoryList = Plugin.TerritoryTypeSheet
            .Where(t => Plugin.AllowedTerritoryUseTypes.Contains(t.TerritoryIntendedUse.RowId) && !string.IsNullOrEmpty(t.PlaceName.Value.Name.ExtractText())
            )
            .OrderBy(x => x.PlaceName.Value.Name.ExtractText())
            .ToDictionary(
                t => t.RowId,
                t => t.PlaceName.Value.Name.ExtractText()
            );

        private static readonly IOrderedEnumerable<IGrouping<string, PropertyInfo>> SystemConfigurationItemsGroup = typeof(SystemConfiguration)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<SystemConfiguration.ConfigurationItemAttribute>() != null)
            .GroupBy(p => p.GetCustomAttribute<SystemConfiguration.ConfigurationItemAttribute>()!.InterfaceGroup)
            .OrderBy(g => g.Key);

        private static readonly IOrderedEnumerable<IGrouping<string, PropertyInfo>> UiConfigurationItemsGroup = typeof(UiConfiguration)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<UiConfiguration.ConfigurationItemAttribute>() != null)
            .GroupBy(p => p.GetCustomAttribute<UiConfiguration.ConfigurationItemAttribute>()!.InterfaceGroup)
            .OrderBy(g => g.Key);


        private string searchTextInput = "";
        private string warningTextInput = "";
        private bool canSaveSettings;
        private SelectedItem? selectedItem;
        public static event Action? ConfigurationSaved;

        public ConfigurationWindow() : base("Settings Enhanced")
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(1000, 600),
            };
            this.Size = new Vector2(1000, 600);
            this.Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
            this.SizeCondition = ImGuiCond.FirstUseEver;
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

        public override void PreOpenCheck()
        {
            if (!Plugin.ClientState.IsLoggedIn)
            {
                this.IsOpen = false;
            }
        }

        public override void Draw()
        {
            if (!Plugin.PluginConfiguration.UiWarningAccepted)
            {
                this.DrawWarning();
                return;
            }

            this.DrawConfigUi();
        }

        private void DrawWarning()
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

            const string mustEnterText = "Trans rights are human rights";
            ImGui.TextWrapped($"Type '{mustEnterText}' to continue.");
            ImGui.SetNextItemWidth(ImGui.CalcTextSize(mustEnterText).X * ImGuiHelpers.GlobalScale);
            ImGui.InputText("##warningTextContinueInput", ref this.warningTextInput, (uint)mustEnterText.Length);
            ImGui.BeginDisabled(neverMadeBackup || !this.warningTextInput.Equals(mustEnterText, StringComparison.OrdinalIgnoreCase));

            ImGui.NewLine();
            if (ImGui.Button("I acknowledge the risks and wish to continue"))
            {
                Plugin.PluginConfiguration.UiWarningAccepted = true;
                Plugin.PluginConfiguration.Save();
            }
            ImGui.EndDisabled();
        }

        private void DrawConfigUi()
        {
            if (ImGui.BeginTable("UiWithSidebar", 2, ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Sidebar", default, ImGui.GetContentRegionAvail().X * 0.25f);
                ImGui.TableSetupColumn("MainContent", default, ImGui.GetContentRegionAvail().X * 0.75f);
                ImGui.TableNextRow();

                // Sidebar
                ImGui.TableNextColumn();
                if (ImGui.BeginChild("SidebarChild", default, true))
                {
                    var filtered = TerritoryList
                        .Where(x => x.Value.Contains(this.searchTextInput, StringComparison.InvariantCultureIgnoreCase));
                    var grouped = filtered
                        .GroupBy(x => Plugin.PluginConfiguration.TerritorySystemConfiguration.ContainsKey((ushort)x.Key))
                        .OrderByDescending(g => g.Key);

                    if (ImGui.BeginChild("SearchbarChild", new(0, (25 * ImGuiHelpers.GlobalScale) - ImGui.GetContentRegionAvail().Y)))
                    {
                        ImGui.SetNextItemWidth(-1);
                        ImGui.InputTextWithHint("##Searchbar", "Search...", ref this.searchTextInput, 50);

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
                                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                                if (ImGui.Selectable($"{name}##{id}", id == this.selectedItem?.TerritoryId))
                                {
                                    this.selectedItem = new()
                                    {
                                        TerritoryId = (ushort)id,
                                        TerritoryName = name,
                                        SystemConfiguration = Plugin.PluginConfiguration.TerritorySystemConfiguration
                                        .GetValueOrDefault(
                                            (ushort)id,
                                            ((SystemConfiguration)Plugin.PluginConfiguration.OriginalSystemConfiguration.Clone()).DepersistAllValues()

                                        ),
                                        UiConfiguration = Plugin.PluginConfiguration.TerritoryUiConfiguration.GetValueOrDefault(
                                            (ushort)id,
                                            ((UiConfiguration)Plugin.PluginConfiguration.OriginalUiConfiguration[Plugin.CurrentPlayerContentId].Clone()).DepersistAllValues()
                                        )
                                    };
                                }
                                if (currentTerritory)
                                    ImGui.PopStyleColor();
                            }
                            ImGuiHelpers.ScaledDummy(8);
                        }
                        ImGui.EndChild();
                    }
                }
            }
            ImGui.EndChild();

            // Listings
            ImGui.TableNextColumn();
            if (ImGui.BeginChild("SidebarDetailChild", default, true))
            {
                if (this.selectedItem is not null)
                {
                    ImGui.TextDisabled(this.selectedItem.TerritoryName);
                    ImGui.Separator();
                    if (ImGui.BeginChild("ConfigurationContent", new(0, ImGui.GetContentRegionAvail().Y - (30 * ImGuiHelpers.GlobalScale))))
                    {
                        if (ImGui.BeginTabBar("ConfigTabs"))
                        {
                            if (ImGui.BeginTabItem("System Configuration"))
                            {
                                foreach (var group in SystemConfigurationItemsGroup)
                                {
                                    if (ImGui.CollapsingHeader(group.Key.ToString()))
                                    {
                                        this.DrawConfigurationGroup<SystemConfiguration, SystemConfiguration.ConfigurationItemAttribute>(group, this.selectedItem.SystemConfiguration);
                                    }
                                }
                                ImGui.EndTabItem();
                            }

                            if (ImGui.BeginTabItem("Character Configuration"))
                            {
                                foreach (var group in UiConfigurationItemsGroup)
                                {
                                    if (ImGui.CollapsingHeader(group.Key.ToString()))
                                    {
                                        this.DrawConfigurationGroup<UiConfiguration, UiConfiguration.ConfigurationItemAttribute>(group, this.selectedItem.UiConfiguration);
                                    }
                                }
                            }
                            ImGui.EndTabItem();
                        }
                        ImGui.EndTabBar();
                    }
                    ImGui.EndChild();
                    ImGui.Separator();

                    if (ImGui.BeginChild("ButtonSection", new Vector2(0, ImGui.GetContentRegionAvail().Y)))
                    {
                        ImGui.BeginDisabled(!this.canSaveSettings);
                        if (ImGui.Button("Apply Settings"))
                        {
                            Plugin.PluginConfiguration.TerritorySystemConfiguration[this.selectedItem.TerritoryId] = this.selectedItem.SystemConfiguration;
                            Plugin.PluginConfiguration.TerritoryUiConfiguration[this.selectedItem.TerritoryId] = this.selectedItem.UiConfiguration;
                            Plugin.PluginConfiguration.Save();
                            ConfigurationSaved?.Invoke();
                            this.canSaveSettings = false;
                        }
                        ImGui.EndDisabled();
                        ImGui.SameLine();
                        ImGui.BeginDisabled(!ImGui.IsKeyDown(ImGuiKey.LeftShift));
                        if (ImGui.Button("Delete Settings"))
                        {
                            this.canSaveSettings = false;

                            // Should never be missing at this point
                            this.selectedItem.SystemConfiguration = ((SystemConfiguration)Plugin.PluginConfiguration.OriginalSystemConfiguration.Clone()).DepersistAllValues();
                            this.selectedItem.UiConfiguration = ((UiConfiguration)Plugin.PluginConfiguration.OriginalUiConfiguration[Plugin.ClientState.LocalContentId].Clone()).DepersistAllValues();
                            Plugin.PluginConfiguration.TerritorySystemConfiguration.Remove(this.selectedItem.TerritoryId);
                            Plugin.PluginConfiguration.TerritoryUiConfiguration.Remove(this.selectedItem.TerritoryId);
                            Plugin.PluginConfiguration.Save();
                            ConfigurationSaved?.Invoke();
                        }
                        ImGui.EndDisabled();
                        ImGuiComponents.HelpMarker("Hold 'Left Shift' to enable deletion. Your original settings will be automatically reapplied.");
                    }
                    ImGui.EndChild();
                }
                ImGui.EndChild();
                ImGui.EndTable();
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

            if (configOptionAttribute?.Nested == true)
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

            if (configOptionAttribute?.Nested == true)
            {
                ImGui.Unindent();
            }
        }

        private void DrawPropertyResetButton<T>(T configuration, PropertyInfo prop) where T : IGameConfiguration<T>
        {
            ImGui.BeginDisabled(!configuration.IsPropertyPersistent(prop));
            if (ImGuiComponents.IconButton(prop.Name, FontAwesomeIcon.Sync))
            {
                if (typeof(T) == typeof(UiConfiguration))
                {
                    if (Plugin.PluginConfiguration.OriginalUiConfiguration.TryGetValue(Plugin.CurrentPlayerContentId, out var uiConfig))
                    {
                        var propertyValue = uiConfig.GetType().GetProperty(prop.Name)?.GetValue(uiConfig);
                        if (propertyValue != null)
                        {
                            configuration.SetProperty(prop, propertyValue);
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
                    var defaultValue = Plugin.PluginConfiguration.OriginalSystemConfiguration.GetType().GetProperty(prop.Name);
                    if (defaultValue is not null)
                    {
                        configuration.SetProperty(prop, defaultValue.GetValue(Plugin.PluginConfiguration.OriginalSystemConfiguration));
                        this.canSaveSettings = true;
                    }
                    else
                    {
                        Plugin.Log.Error($"Failed to reset system setting: {prop.Name} does not have value on original system configuration");
                    }
                }
            }
            ImGui.EndDisabled();
        }

        private void DrawEnumProperty<T>(T configuration, PropertyInfo prop, string displayName) where T : IGameConfiguration<T>
        {
            var enumValues = Enum.GetValues(prop.PropertyType).Cast<Enum>().ToArray();
            var value = prop.GetValue(configuration) as Enum;
            if (value is null && ImGui.Selectable("None", value is null))
            {
                configuration.SetPropertyPersistent<Enum?>(prop, null);
            }
            else if (ImGui.BeginCombo(displayName, value?.ToString() ?? "None"))
            {
                foreach (var enumValue in enumValues)
                {
                    var isSelected = enumValue.Equals(value);
                    if (ImGui.Selectable(enumValue.ToString(), isSelected))
                    {
                        configuration.SetPropertyPersistent(prop, Enum.ToObject(prop.PropertyType, enumValue));
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
                configuration.SetPropertyPersistent(prop, currentValue);
                this.canSaveSettings = true;
            }
        }

        private void DrawUintProperty<T>(T configuration, PropertyInfo prop, string displayName) where T : IGameConfiguration<T>
        {
            var range = prop.GetCustomAttribute<ConfigurationInputRangeAttribute>() ?? new(0, 100);
            var currentValue = (uint)(prop.GetValue(configuration) ?? 0);
            var refValue = (int)currentValue;
            if (ImGui.SliderInt(displayName, ref refValue, range.Min, range.Max))
            {
                configuration.SetPropertyPersistent(prop, (uint)refValue);
                this.canSaveSettings = true;
            }
        }

        private void DrawBoolProperty<T>(T configuration, PropertyInfo prop, string displayName) where T : IGameConfiguration<T>
        {
            var currentValue = (bool)(prop.GetValue(configuration) ?? false);
            if (ImGui.Checkbox(displayName, ref currentValue))
            {
                configuration.SetPropertyPersistent(prop, currentValue);
                this.canSaveSettings = true;
            }
        }
    }
}
