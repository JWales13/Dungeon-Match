namespace Game.Core
{
    /// <summary>The geometric kind of a detected match group.</summary>
    public enum MatchShape
    {
        Line,   // 3+ in a straight row or column
        Square  // a 2x2 block of one color
    }
}