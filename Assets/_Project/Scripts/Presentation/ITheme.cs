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