using System;

namespace Game.Core
{
    /// <summary>
    /// A pluggable win/lose rule that sits above the Board. The Board doesn't
    /// know objectives exist; a driver feeds resolved-clear info in.
    ///
    /// Note: this reports tiles CLEARED (for damage/score). Consuming a "move"
    /// is a separate, objective-specific concern (see MonsterCombatObjective's
    /// SpendMove) - because clears can come from things that aren't a player
    /// move, like a crafted booster.
    /// </summary>
    public interface IBoardObjective
    {
        ObjectiveStatus Status { get; }

        event Action<ObjectiveStatus> StatusChanged;

        /// <summary>
        /// Called once per fully-resolved board settle, with what was cleared.
        /// </summary>
        void RegisterClears(MoveOutcome move);
    }
}