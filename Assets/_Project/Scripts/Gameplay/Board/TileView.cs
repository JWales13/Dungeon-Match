using UnityEngine;
using Game.Core;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>
    /// The visual representation of a single cell. Displays a Tile: normal tiles
    /// are a full-size colored square; power tiles are brighter and shaped to
    /// hint their effect (column-clearer = vertical bar, row-clearer =
    /// horizontal bar, mortar = small square). All values come from the active
    /// Theme, so this script never changes when the look is retuned.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TileView : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Display(Tile tile)
        {
            EnsureRenderer();

            ITheme theme = Theme.Current;
            Color baseColor = theme.GetTileColor(tile.Type);

            if (!tile.IsPowerTile)
            {
                _spriteRenderer.color = baseColor;
                transform.localScale = Vector3.one * theme.TileScale;
                return;
            }

            _spriteRenderer.color = Color.Lerp(baseColor, theme.MatchedFlashColor, theme.PowerTileHighlightAmount);
            transform.localScale = PowerTileScale(tile.Power, theme);
        }

        /// <summary>
        /// MVP placeholder for a match/clear reaction. Replace with a real
        /// animation/particle once the loop is proven fun.
        /// </summary>
        public void PlayMatchedEffect()
        {
            EnsureRenderer();
            _spriteRenderer.color = Theme.Current.MatchedFlashColor;
        }

        private static Vector3 PowerTileScale(PowerTileKind kind, ITheme theme)
        {
            float thin = theme.PowerTileThickness;
            float longEdge = theme.TileScale;

            switch (kind)
            {
                case PowerTileKind.ClearColumn: return new Vector3(thin, longEdge, 1f);  // vertical bar
                case PowerTileKind.ClearRow:    return new Vector3(longEdge, thin, 1f);   // horizontal bar
                case PowerTileKind.Mortar:      return new Vector3(thin, thin, 1f);       // small square
                default:                        return Vector3.one * longEdge;
            }
        }

        private void EnsureRenderer()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }
    }
}