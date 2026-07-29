using System;

namespace Game.Core
{
    /// <summary>
    /// How deep into the tower the player has descended. Starts at depth 1.
    /// AdvanceDepth is called exactly once per floor actually won - never on
    /// Continue or Retry, which replay the same depth - matching "no
    /// replaying cleared floors" (every win is progress, never a retread).
    /// Never decreases.
    /// </summary>
    public class TowerProgress
    {
        public int CurrentDepth { get; private set; }

        public event Action Changed;

        public TowerProgress(int initialDepth = 1)
        {
            if (initialDepth < 1) throw new ArgumentOutOfRangeException(nameof(initialDepth));
            CurrentDepth = initialDepth;
        }

        public void AdvanceDepth()
        {
            CurrentDepth++;
            Changed?.Invoke();
        }
    }
}
