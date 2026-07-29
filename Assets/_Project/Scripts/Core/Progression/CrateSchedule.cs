using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// How many crates a floor's board should have, by depth: none before
    /// startingDepth, then a percentage of the board's cells, growing
    /// gradually and capped so the board never becomes unplayable.
    /// </summary>
    public class CrateSchedule
    {
        private readonly int _startingDepth;
        private readonly float _basePercentage;
        private readonly float _percentagePerDepth;
        private readonly float _maxPercentage;

        public CrateSchedule(int startingDepth, float basePercentage, float percentagePerDepth, float maxPercentage)
        {
            if (startingDepth < 1) throw new ArgumentOutOfRangeException(nameof(startingDepth));
            if (basePercentage < 0f || basePercentage > 1f) throw new ArgumentOutOfRangeException(nameof(basePercentage));
            if (percentagePerDepth < 0f) throw new ArgumentOutOfRangeException(nameof(percentagePerDepth));
            if (maxPercentage < basePercentage || maxPercentage > 1f) throw new ArgumentOutOfRangeException(nameof(maxPercentage));

            _startingDepth = startingDepth;
            _basePercentage = basePercentage;
            _percentagePerDepth = percentagePerDepth;
            _maxPercentage = maxPercentage;
        }

        /// <summary>How many cells of a boardWidth x boardHeight board should be crates at this depth (0 before startingDepth).</summary>
        public int CrateCountFor(int depth, int boardWidth, int boardHeight)
        {
            if (depth < 1) throw new ArgumentOutOfRangeException(nameof(depth));
            if (boardWidth <= 0) throw new ArgumentOutOfRangeException(nameof(boardWidth));
            if (boardHeight <= 0) throw new ArgumentOutOfRangeException(nameof(boardHeight));

            if (depth < _startingDepth)
            {
                return 0;
            }

            int depthsIn = depth - _startingDepth;
            float percentage = Mathf.Min(_maxPercentage, _basePercentage + _percentagePerDepth * depthsIn);
            int boardArea = boardWidth * boardHeight;

            return Mathf.RoundToInt(boardArea * percentage);
        }
    }
}
