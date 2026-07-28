using UnityEngine;
using TMPro;
using Game.Core;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>
    /// Shows the current ingredient stash (one line per color). Purely a
    /// presenter: it subscribes to the inventory's Changed event and writes
    /// text; names come from the theme.
    /// </summary>
    public class IngredientHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private static readonly TileType[] DisplayOrder =
        {
            TileType.Red, TileType.Green, TileType.Yellow, TileType.Blue
        };

        private IngredientInventory _inventory;

        public void Initialize(IngredientInventory inventory)
        {
            Unsubscribe();
            _inventory = inventory;
            ApplyStyle();
            _inventory.Changed += Refresh;
            Refresh();
        }

        private void ApplyStyle()
        {
            if (_text == null)
            {
                return;
            }

            ITheme theme = Theme.Current;
            _text.fontSize = theme.CaptionFontSize;
            _text.color = theme.HudTextColor;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (_inventory != null)
            {
                _inventory.Changed -= Refresh;
            }
        }

        private void Refresh()
        {
            if (_text == null)
            {
                return;
            }

            ITheme theme = Theme.Current;
            var lines = new string[DisplayOrder.Length];
            for (int i = 0; i < DisplayOrder.Length; i++)
            {
                TileType color = DisplayOrder[i];
                lines[i] = $"{theme.GetIngredientName(color)}: {_inventory.GetCount(color)}";
            }

            _text.text = string.Join("\n", lines);
        }
    }
}