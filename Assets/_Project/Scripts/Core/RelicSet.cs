using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// The relics the player currently holds this run. Applies every relic's
    /// hooks in the order acquired. Combat asks the set to modify a value; it
    /// never inspects individual relics, so adding relic types never touches
    /// this class.
    /// </summary>
    public class RelicSet
    {
        private readonly List<IRelic> _relics = new List<IRelic>();

        public IReadOnlyList<IRelic> Relics => _relics;
        public int Count => _relics.Count;

        public void Add(IRelic relic)
        {
            if (relic == null) throw new ArgumentNullException(nameof(relic));
            _relics.Add(relic);
        }

        public int ModifyMoveLimit(int baseMoveLimit)
        {
            int value = baseMoveLimit;
            foreach (IRelic relic in _relics)
            {
                value = relic.ModifyMoveLimit(value);
            }

            return value;
        }

        public int ModifyMoveDamage(int baseDamage, MoveOutcome move)
        {
            int value = baseDamage;
            foreach (IRelic relic in _relics)
            {
                value = relic.ModifyMoveDamage(value, move);
            }

            return value;
        }
    }
}