using System;

namespace Game.Core
{
    /// <summary>
    /// A pluggable win/lose rule that sits above the Board. The Board doesn't
    /// know objectives exist; a driver feeds resolved-move info into whichever
    /// objective is active. This is the seam that lets us swap "drain monster
    /// HP" for "hit a score target" or "collect N red tiles" without touching
    /// board or rendering code.
    /// </summary>
    public interface IBoardObjective
    {
        ObjectiveStatus Status { get; }

        event Action<ObjectiveStatus> StatusChanged;

        /// <summary>
        /// Called once per fully-resolved player move (after all cascades
        /// settle), with the total number of tiles cleared during that move.
        /// </summary>
        void RegisterResolvedMove(int tilesClearedThisMove);
    }
}