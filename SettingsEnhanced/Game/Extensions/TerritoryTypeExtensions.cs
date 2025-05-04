using SettingsEnhanced.Game.Enums;

namespace SettingsEnhanced.Game.Extensions
{
    internal static class TerritoryTypeExtensions
    {
        public static string GetTerritoryName(this Lumina.Excel.Sheets.TerritoryType t) => t.TerritoryIntendedUse.RowId switch
        {
            (uint)TerritoryIntendedUse.Dungeon or (uint)TerritoryIntendedUse.Trial or (uint)TerritoryIntendedUse.AllianceRaid or (uint)TerritoryIntendedUse.Raids or (uint)TerritoryIntendedUse.RaidFights => t.ContentFinderCondition.Value.Name.ExtractText(),
            _ => t.PlaceName.Value.Name.ExtractText(),
        };
    }
}
