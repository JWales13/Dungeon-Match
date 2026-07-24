using UnityEngine;
using Game.Core;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>
    /// The visual representation of a single cell. Knows how to display a
    /// TileType, nothing more. All colors come from the active Theme, so this
    /// script never needs editing when the palette changes.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TileView : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetType(TileType type)
        {
            _spriteRenderer.color = Theme.Current.GetTileColor(type);
        }

        public void PlayMatchedEffect()
        {
            _spriteRenderer.color = Theme.Current.MatchedFlashColor;
        }
    }
}