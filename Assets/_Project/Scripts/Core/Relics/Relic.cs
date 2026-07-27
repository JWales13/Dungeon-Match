namespace Game.Core
{
    /// <summary>
    /// Base class for relics. Provides identity pass-through hooks so each
    /// concrete relic overrides only the one behavior it changes, keeping every
    /// relic tiny and single-purpose.
    /// </summary>
    public abstract class Relic : IRelic
    {
        public abstract string DisplayName { get; }
        public abstract string Description { get; }

        public virtual int ModifyMoveLimit(int baseMoveLimit) => baseMoveLimit;

        public virtual int ModifyMoveDamage(int baseDamage, MoveOutcome move) => baseDamage;
    }
}