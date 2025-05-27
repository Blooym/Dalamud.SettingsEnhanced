using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Dalamud.Game.Config;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using SettingsEnhanced.Game.Extensions;
using SettingsEnhanced.Game.Settings;
using SettingsEnhanced.Game.Settings.Attributes;
using SettingsEnhanced.Game.Settings.Interfaces;
using SettingsEnhanced.Resources.Localization;
using SystemConfiguration = SettingsEnhanced.Game.Settings.SystemConfiguration;

namespace SettingsEnhanced.UI.Windows
{
    internal sealed class ConfigurationWindow : Window
    {
        private sealed class SelectedItem
        {
            public required ushort TerritoryId;
            public required string TerritoryName;
            public required SystemConfiguration SystemConfiguration;
            public required UiConfiguration UiConfiguration;
        }

        private const ImGuiWindowFlags NoScrollFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        private static readonly Dictionary<uint, string> TerritoryList = Plugin.EnabledTerritories
            .Select(t => new { t.RowId, TerritoryName = t.GetName() })
            .Where(t => !string.IsNullOrEmpty(t.TerritoryName))
            .OrderBy(t => t.TerritoryName)
            .ToDictionary(t => t.RowId, t => t.TerritoryName);
        private static readonly ImmutableArray<IGrouping<string, (PropertyInfo, UiSettingPropDisplayAttribute)>> SystemConfigurationGroups =
            [.. typeof(SystemConfiguration)
                .GetProperties(Plugin.ConfigReflectionBindingFlags)
                .Select(p => (Property: p, Attribute: p.GetCustomAttribute<UiSettingPropDisplayAttribute>()!))
                .Where(x => x.Attribute is not  null)
                .GroupBy(x => x.Attribute.UiGroup)
                .OrderBy(g => g.Key)];
        private static readonly ImmutableArray<IGrouping<string, (PropertyInfo, UiSettingPropDisplayAttribute)>> UiConfigurationGroups =
            [.. typeof(UiConfiguration)
                .GetProperties(Plugin.ConfigReflectionBindingFlags)
                .Select(p => (Property: p, Attribute: p.GetCustomAttribute<UiSettingPropDisplayAttribute>()!))
                .Where(x => x.Attribute is not  null)
                .GroupBy(x => x.Attribute.UiGroup)
                .OrderBy(g => g.Key)];
        private static readonly Dictionary<Type, Dictionary<Enum, string>> EnumAddonTextCache =
            Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsEnum)
                .Select(enumType => new
                {
                    EnumType = enumType,
                    Names = Enum.GetValues(enumType).Cast<Enum>().ToDictionary(
                        ev => ev,
                        ev =>
                        {
                            var member = enumType.GetMember(ev.ToString()).FirstOrDefault();
                            return member?.GetCustomAttribute<UiSettingEnumAddonIdAttribute>()?.UiName ?? ev.ToString();
                        })
                })
                .ToDictionary(x => x.EnumType, x => x.Names);

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
            this.Flags = NoScrollFlags;
            this.SizeCondition = ImGuiCond.FirstUseEver;
            this.AllowClickthrough = false;
            this.AllowPinning = false;
            this.TitleBarButtons = [
                new()
                {
                    Icon = FontAwesomeIcon.Heart,
                    Click= (_) => Util.OpenLink("https://go.blooym.dev/donate"),
                    ShowTooltip = () => ImGui.SetTooltip(Strings.UI_Titlebar_SupportDeveloper),
                },
                new() {
                    Icon = FontAwesomeIcon.Comment,
                    Click = (_) => Util.OpenLink("https://github.com/Blooym/Dalamud.SettingsEnhanced"),
                    ShowTooltip = () => ImGui.SetTooltip(Strings.UI_Titlebar_Repository),
                },
            ];
        }

        public override bool DrawConditions() => Plugin.ClientState.IsLoggedIn;

        public override void OnClose()
        {
            this.selectedItem = null;
            this.canSaveSettings = false;
            this.searchText = "";
            base.OnClose();
        }

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
            ImGui.TextColored(ImGuiColors.DalamudRed, Strings.UI_Configuration_WarningTitle);
            ImGui.TextWrapped(Strings.UI_Configuration_WarningText1);
            ImGui.TextWrapped(Strings.UI_Configuration_WarningText2);
            ImGui.TextColored(ImGuiColors.DalamudYellow, Strings.UI_Configuration_WarningText3);
            ImGui.TextWrapped(Strings.UI_Configuration_WarningText4);
            ImGui.TextWrapped(Strings.UI_Configuration_WarningText5);
            ImGui.NewLine();
            if (neverMadeBackup)
            {
                ImGui.TextColored(ImGuiColors.DalamudRed, Strings.UI_Configuration_WarningBackupRequired);
                return;
            }
            using (ImRaii.Disabled(neverMadeBackup || !ImGui.IsKeyDown(ImGuiKey.LeftShift)))
            {
                if (ImGui.Button(Strings.UI_Configuration_WarningContinueButton))
                {
                    Plugin.PluginConfiguration.UiWarningAccepted = true;
                    Plugin.PluginConfiguration.Save();
                }
            }
            ImGuiComponents.HelpMarker(Strings.UI_Configuration_WarningContinueHint);
        }

        private void DrawConfigUi()
        {
            using (var uiMainChild = ImRaii.Child("UiWithSidebarChild", new(default, ImGui.GetContentRegionAvail().Y - (20 * ImGuiHelpers.GlobalScale)), false, NoScrollFlags))
            {
                using var sidebarTable = ImRaii.Table("UiWithSidebarTable", 2);
                if (sidebarTable)
                {
                    ImGui.TableSetupColumn("Sidebar", ImGuiTableColumnFlags.WidthFixed, ImGui.GetContentRegionAvail().X * 0.28f);
                    ImGui.TableSetupColumn("Main", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    using (var sidebarChild = ImRaii.Child("SidebarChild", default, true))
                    {
                        if (sidebarChild)
                        {
                            this.DrawSidebar();
                        }
                    }
                    ImGui.TableNextColumn();
                    using var mainContentChild = ImRaii.Child("MainContent", default, true, NoScrollFlags);
                    if (mainContentChild)
                    {
                        this.DrawMainContent();
                    }
                }
            }
            if (Plugin.PluginConfiguration.UiConfigurationOverwritten || Plugin.PluginConfiguration.SystemConfigurationOverwritten)
            {
                ImGuiHelpers.SafeTextColoredWrapped(ImGuiColors.DalamudYellow, Strings.UI_Configuration_SettingsType_Zone_Title);
                ImGuiComponents.HelpMarker(Strings.UI_Configuration_SettingsType_Zone_Description, FontAwesomeIcon.QuestionCircle);
            }
            else
            {
                ImGuiHelpers.SafeTextColoredWrapped(ImGuiColors.HealerGreen, Strings.UI_Configuration_SettingsType_Game_Title);
                ImGuiComponents.HelpMarker(Strings.UI_Configuration_SettingsType_Game_Description, FontAwesomeIcon.QuestionCircle);
            }
        }

        private void DrawSidebar()
        {
            using (var searchbarChild = ImRaii.Child("SearchbarChild", new(0, (25 * ImGuiHelpers.GlobalScale) - ImGui.GetContentRegionAvail().Y)))
            {
                if (searchbarChild)
                {
                    ImGui.SetNextItemWidth(-1);
                    ImGui.InputTextWithHint("##Searchbar", "Search...", ref this.searchText, 50);
                }
            }
            ImGui.Separator();
            using var searchResultsChild = ImRaii.Child("SearchResultsChild");
            if (searchResultsChild)
            {
                foreach (var group in TerritoryList
                    .Where(x => x.Value.Contains(this.searchText, StringComparison.InvariantCultureIgnoreCase))
                    .GroupBy(x => Plugin.PluginConfiguration.TerritorySystemConfiguration.ContainsKey((ushort)x.Key)
                                || Plugin.PluginConfiguration.TerritoryUiConfiguration.ContainsKey((ushort)x.Key))
                    .OrderByDescending(g => g.Key))
                {
                    var hasSettings = group.Key;
                    ImGui.TextDisabled(hasSettings ? Strings.UI_Configuration_Zonelist_CustomSettings : Strings.UI_Configuration_Zonelist_DefaultSettings);
                    ImGuiClip.ClippedDraw(group.ToImmutableList(), this.DrawTerritorySelectable, ImGui.GetTextLineHeightWithSpacing());
                }
            }
        }

        private void DrawTerritorySelectable(KeyValuePair<uint, string> item)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudViolet, item.Key == Plugin.ClientState.TerritoryType))
            {
                if (ImGui.Selectable($"{item.Value}##{item.Key}", item.Key == this.selectedItem?.TerritoryId))
                {
                    this.selectedItem = new()
                    {
                        TerritoryId = (ushort)item.Key,
                        TerritoryName = item.Value,
                        SystemConfiguration = Plugin.PluginConfiguration.TerritorySystemConfiguration
                        .GetValueOrDefault(
                            (ushort)item.Key,
                            ((SystemConfiguration)Plugin.PluginConfiguration.OriginalSystemConfiguration.Clone()).DepersistAllProperties()

                        ),
                        UiConfiguration = Plugin.PluginConfiguration.TerritoryUiConfiguration.GetValueOrDefault(
                            (ushort)item.Key,
                            ((UiConfiguration)Plugin.PluginConfiguration.OriginalUiConfiguration[Plugin.CurrentPlayerContentId].Clone()).DepersistAllProperties()
                        )
                    };
                }
            }
        }

        private void DrawMainContent()
        {
            if (this.selectedItem is not null)
            {
                ImGui.TextDisabled($"{this.selectedItem.TerritoryName} - [ID {this.selectedItem.TerritoryId}]");
                ImGui.Separator();
                using (var configContentChild = ImRaii.Child("ConfigurationContent", new(default, ImGui.GetContentRegionAvail().Y - (26 * ImGuiHelpers.GlobalScale))))
                {
                    if (configContentChild)
                    {
                        using (var configTabBar = ImRaii.TabBar("ConfigTabs"))
                        {
                            if (configTabBar)
                            {
                                using (var tabItem = ImRaii.TabItem(Strings.UI_Configuration_ZoneConfig_SystemConfig))
                                {
                                    if (tabItem)
                                    {
                                        using var configChild = ImRaii.Child("SystemConfChild");
                                        if (configChild)
                                        {
                                            foreach (var group in SystemConfigurationGroups)
                                            {
                                                if (ImGui.CollapsingHeader(group.Key))
                                                {
                                                    this.DrawConfigurationGroup(group, this.selectedItem.SystemConfiguration);
                                                }
                                            }
                                        }
                                    }
                                }

                                using (var tabItem = ImRaii.TabItem(Strings.UI_Configuration_ZoneConfig_CharaConfig))
                                {
                                    if (tabItem)
                                    {
                                        using var configChild = ImRaii.Child("CharConfigChild");
                                        if (configChild)
                                        {
                                            foreach (var group in UiConfigurationGroups)
                                            {
                                                if (ImGui.CollapsingHeader(group.Key))
                                                {
                                                    this.DrawConfigurationGroup(group, this.selectedItem.UiConfiguration);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                ImGui.Separator();

                using (ImRaii.Disabled(!this.canSaveSettings))
                {
                    if (ImGui.Button(Strings.UI_Configuration_ZoneConfig_ApplyButton))
                    {
                        // Set or remove configurations depending on if they have any persists left.
                        if (this.selectedItem.SystemConfiguration.AnyPersistedProperties())
                        {
                            Plugin.PluginConfiguration.TerritorySystemConfiguration[this.selectedItem.TerritoryId] = this.selectedItem.SystemConfiguration;
                        }
                        else
                        {
                            Plugin.PluginConfiguration.TerritorySystemConfiguration.Remove(this.selectedItem.TerritoryId);
                        }

                        if (this.selectedItem.UiConfiguration.AnyPersistedProperties())
                        {
                            Plugin.PluginConfiguration.TerritoryUiConfiguration[this.selectedItem.TerritoryId] = this.selectedItem.UiConfiguration;
                        }
                        else
                        {
                            Plugin.PluginConfiguration.TerritoryUiConfiguration.Remove(this.selectedItem.TerritoryId);
                        }

                        Plugin.PluginConfiguration.Save();
                        ConfigurationUpdated?.Invoke();
                        this.canSaveSettings = false;
                    }
                }
                ImGui.SameLine();
                using (ImRaii.Disabled(!ImGui.IsKeyDown(ImGuiKey.LeftShift)))
                {
                    if (ImGui.Button(Strings.UI_Configuration_ZoneConfig_DeleteButton))
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
                }
                ImGuiComponents.HelpMarker(Strings.UI_Configuration_ZoneConfig_DeleteButton_Hint);
            }
        }

        private void DrawConfigurationGroup<TConfig>(IGrouping<string, (PropertyInfo propInfo, UiSettingPropDisplayAttribute displayInfo)> group, TConfig configuration) where TConfig : IGameConfiguration<TConfig>
        {
            var subGroups = group
                .GroupBy(p => p.displayInfo.UiHeader)
                .OrderBy(g => g.Key);
            foreach (var subGroup in subGroups)
            {
                if (!string.IsNullOrWhiteSpace(subGroup.Key))
                {
                    ImGui.TextDisabled(subGroup.Key);
                }
                foreach (var (propInfo, displayInfo) in subGroup)
                {
                    this.DrawConfigurationProperty(propInfo, displayInfo, configuration);
                }
                ImGuiHelpers.ScaledDummy(6);
            }
        }

        private void DrawConfigurationProperty<TConfig>(PropertyInfo prop, UiSettingPropDisplayAttribute display, TConfig configuration) where TConfig : IGameConfiguration<TConfig>
        {
            using (ImRaii.PushIndent(15f, true, display.UiIndented))
            {
                this.DrawPropertyResetButton(configuration, prop);
                ImGui.SameLine();
                if (prop.PropertyType.IsEnum)
                {
                    this.DrawEnumProperty(configuration, prop, display.UiName);
                }
                else if (prop.PropertyType == typeof(uint))
                {
                    this.DrawUintProperty(configuration, prop, display.UiName);
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    this.DrawBoolProperty(configuration, prop, display.UiName);
                }
                else if (prop.PropertyType == typeof(string))
                {
                    this.DrawStringProperty(configuration, prop, display.UiName);
                }
            }
        }

        private void DrawPropertyResetButton<T>(T configuration, PropertyInfo prop) where T : IGameConfiguration<T>
        {
            using (ImRaii.Disabled(!configuration.IsPropertyPersisted(prop)))
            {
                if (ImGuiComponents.IconButton(prop.Name, FontAwesomeIcon.Sync))
                {
                    if (typeof(T) == typeof(UiConfiguration))
                    {
                        if (Plugin.PluginConfiguration.OriginalUiConfiguration.TryGetValue(Plugin.CurrentPlayerContentId, out var uiConfig))
                        {
                            var propertyValue = typeof(UiConfiguration).GetProperty(prop.Name)?.GetValue(uiConfig);
                            if (propertyValue is not null)
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
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(Strings.UI_Configuration_ZoneConfig_ResetToDefault);
            }
        }

        private void DrawEnumProperty<T>(T configuration, PropertyInfo prop, string displayName) where T : IGameConfiguration<T>
        {
            if (!EnumAddonTextCache.TryGetValue(prop.PropertyType, out var cacheItem))
            {
                ImGuiHelpers.SafeTextColoredWrapped(ImGuiColors.DalamudRed, $"Failed to read text cache entry for {prop.PropertyType}");
                return;
            }
            var enumValue = prop.GetValue(configuration) as Enum;
            var displayValue = (enumValue is not null && cacheItem.TryGetValue(enumValue, out var name))
                ? name
                : Strings.UI_Configuration_ZoneConfig_EnumFallback;

            using var combo = ImRaii.Combo(displayName, displayValue);
            if (combo)
            {
                foreach (var entry in cacheItem)
                {
                    if (ImGui.Selectable(entry.Value))
                    {
                        configuration.SetPropertyValue(prop, Enum.ToObject(prop.PropertyType, entry.Key));
                        configuration.PersistProperty(prop);
                        this.canSaveSettings = true;
                    }
                }
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
