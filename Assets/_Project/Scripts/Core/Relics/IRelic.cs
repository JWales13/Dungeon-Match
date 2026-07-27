namespace Game.Core
{
    /// <summary>
    /// A run modifier. Each relic can adjust the room's move limit and/or the
    /// damage of a resolved move. Relics are pure and stackable - the RelicSet
    /// runs each active relic's hooks in turn. New relic ideas are new
    /// implementations; combat code never changes (Open/Closed).
    /// </summary>
    public interface IRelic
    {
        string DisplayName { get; }
        string Description { get; }

        int ModifyMoveLimit(int baseMoveLimit);
        int ModifyMoveDamage(int baseDamage, MoveOutcome move);
    }
}