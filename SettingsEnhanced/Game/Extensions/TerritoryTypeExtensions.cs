using System.Globalization;
using SettingsEnhanced.Game.Enums;

namespace SettingsEnhanced.Game.Extensions
{
    internal static class TerritoryTypeExtensions
    {
        public static string GetName(this Lumina.Excel.Sheets.TerritoryType t) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.TerritoryIntendedUse.RowId switch
        {
            // Content finder content
            (uint)TerritoryIntendedUse.Dungeon or (uint)TerritoryIntendedUse.Trial or
            (uint)TerritoryIntendedUse.AllianceRaid or (uint)TerritoryIntendedUse.Raids or
            (uint)TerritoryIntendedUse.RaidFights or (uint)TerritoryIntendedUse.PalaceOfTheDead or
            (uint)TerritoryIntendedUse.VariantDungeon or (uint)TerritoryIntendedUse.CriterionDungeon or
            (uint)TerritoryIntendedUse.OccultCrescent or (uint)TerritoryIntendedUse.CriterionDungeonSavage or
            (uint)TerritoryIntendedUse.Bozja or (uint)TerritoryIntendedUse.Eureka => t.ContentFinderCondition.Value.Name.ExtractText(),
            // Fallback
            _ => t.PlaceName.Value.Name.ExtractText(),
        });
    }
}
