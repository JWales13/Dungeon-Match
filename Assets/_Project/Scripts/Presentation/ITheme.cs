using UnityEngine;
using Game.Core;

namespace Game.Presentation
{
    /// <summary>
    /// The single contract for every visual value in the game: colors, sizes,
    /// spacing, and player-facing result strings. Views depend on this
    /// interface (via Theme.Current), never on hard-coded colors. To restyle
    /// the whole game you write one new ITheme, or edit DefaultTheme - no view
    /// script changes, no Inspector edits.
    /// </summary>
    public interface ITheme
    {
        // --- Board / camera ---
        Color BackgroundColor { get; }
        float CellSize { get; }
        float TileScale { get; }

        // --- Tiles ---
        Color GetTileColor(TileType type);
        Color MatchedFlashColor { get; }

        // --- Ingredients ---
        // The display name for a color's crafting ingredient.
        string GetIngredientName(TileType color);

        // --- Power tiles ---
        // How far a power tile's color is blended toward MatchedFlashColor (0-1).
        float PowerTileHighlightAmount { get; }
        // The "thin" edge of a power tile's shape (its long edge is TileScale),
        // used to draw column-clearers as vertical bars, row-clearers as
        // horizontal bars, and mortars as small squares.
        float PowerTileThickness { get; }

        // --- HUD text ---
        Color HudTextColor { get; }
        float HudLabelFontSize { get; }

        // --- Result banner (single board) ---
        float ResultFontSize { get; }
        Color VictoryColor { get; }
        Color DefeatColor { get; }
        string VictoryMessage { get; }
        string DefeatMessage { get; }

        // --- Run flow ---
        string RoomClearedMessage { get; }
        string RunVictoryMessage { get; }
        string RunDefeatMessage { get; }
    }
}