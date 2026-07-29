using UnityEngine;
using Game.Core;

namespace Game.Presentation
{
    /// <summary>
    /// The current visual identity of the game. THIS is the one file to edit
    /// for palette / typography / spacing changes. Colors are written as
    /// Color32 (0-255 per channel) so they map directly to the hex values in
    /// the style guide. Adding a new theme later = copy this class, change the
    /// values, and set Theme.Current to it.
    /// </summary>
    public class DefaultTheme : ITheme
    {
        // Board / camera
        public Color BackgroundColor => new Color32(24, 22, 28, 255);   // #18161C near-black
        public float CellSize => 1f;
        public float TileScale => 0.9f;

        // Tiles
        public Color GetTileColor(TileType type)
        {
            switch (type)
            {
                case TileType.Red:    return new Color32(224, 82, 77, 255);    // #E0524D
                case TileType.Blue:   return new Color32(77, 139, 224, 255);   // #4D8BE0
                case TileType.Green:  return new Color32(95, 184, 92, 255);    // #5FB85C
                case TileType.Yellow: return new Color32(232, 195, 61, 255);   // #E8C33D
                default:              return Color.magenta;                    // loud "unmapped" flag
            }
        }

        public Color MatchedFlashColor => Color.white;

        // Ingredients (one per color)
        public string GetIngredientName(TileType color)
        {
            switch (color)
            {
                case TileType.Red:    return "Gunpowder";
                case TileType.Green:  return "Toxic Goo";
                case TileType.Yellow: return "Live Wire";
                case TileType.Blue:   return "Rations";
                default:              return "?";
            }
        }

        // Boosters (one per Producer station)
        public string GetBoosterName(BoosterType type)
        {
            switch (type)
            {
                case BoosterType.Dynamite:   return "Dynamite";
                case BoosterType.AcidVial:   return "Acid Vial";
                case BoosterType.Overcharge: return "Overcharge";
                case BoosterType.EnergyDrink: return "Energy Drink";
                default:                     return "?";
            }
        }

        // Currencies
        public string GetCurrencyName(CurrencyType currency)
        {
            switch (currency)
            {
                case CurrencyType.Gold:         return "Gold";
                case CurrencyType.PrizeVoucher: return "Prize Vouchers";
                default:                        return "?";
            }
        }

        // Power tiles
        public float PowerTileHighlightAmount => 0.45f;
        public float PowerTileThickness => 0.4f;

        // HUD text
        public Color HudTextColor => new Color32(237, 237, 237, 255);   // #EDEDED off-white
        public float HudLabelFontSize => 28f;
        public float CaptionFontSize => 22f;

        // Result banner
        public float ResultFontSize => 60f;
        public Color VictoryColor => new Color32(111, 207, 96, 255);    // #6FCF60
        public Color DefeatColor => new Color32(214, 69, 69, 255);      // #D64545
        public string VictoryMessage => "VICTORY";
        public string DefeatMessage => "ELIMINATED";

        // Floor result
        public string FloorVictoryMessage => "YOU SURVIVED";
        public string FloorDefeatMessage => "ELIMINATED";
    }
}