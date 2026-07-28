using System;

namespace Game.Core
{
    /// <summary>
    /// Turns power-tile detonations into harvested ingredients: each time a
    /// power tile goes off, its color's ingredient is added to the inventory.
    /// This is the single wiring point between the board and the stash, so
    /// neither has to know about the other. One responsibility.
    /// </summary>
    public class IngredientHarvester
    {
        private readonly IngredientInventory _inventory;
        private readonly int _yieldPerDetonation;

        public IngredientHarvester(Board board, IngredientInventory inventory, int yieldPerDetonation)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _yieldPerDetonation = yieldPerDetonation > 0 ? yieldPerDetonation : 1;

            board.PowerTileDetonated += HandlePowerTileDetonated;
        }

        private void HandlePowerTileDetonated(TileType color)
        {
            _inventory.Add(color, _yieldPerDetonation);
        }
    }
}