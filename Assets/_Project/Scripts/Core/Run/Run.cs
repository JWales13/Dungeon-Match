using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// The roguelite run: an ordered list of rooms plus the player's position
    /// in it. Owns exactly the run-progression rules (advance on win, end on
    /// final win or any loss) and nothing about boards, combat, or rendering.
    /// Pure C#, so the whole progression is unit tested without Unity.
    /// </summary>
    public class Run
    {
        private readonly IReadOnlyList<RoomDefinition> _rooms;

        public int CurrentRoomIndex { get; private set; }
        public RunStatus Status { get; private set; } = RunStatus.InProgress;

        public int TotalRooms => _rooms.Count;
        public int CurrentRoomNumber => CurrentRoomIndex + 1;
        public RoomDefinition CurrentRoom => _rooms[CurrentRoomIndex];
        public bool IsFinalRoom => CurrentRoomIndex == _rooms.Count - 1;

        public event Action<RunStatus> StatusChanged;
        public event Action<int, int> RoomChanged; // (roomNumber, totalRooms)

        public Run(IReadOnlyList<RoomDefinition> rooms)
        {
            if (rooms == null) throw new ArgumentNullException(nameof(rooms));
            if (rooms.Count == 0) throw new ArgumentException("A run needs at least one room.", nameof(rooms));

            _rooms = rooms;
        }

        /// <summary>
        /// Records the outcome of the current room. Winning a non-final room
        /// advances to the next; winning the final room wins the run; losing
        /// any room ends the run.
        /// </summary>
        public void RegisterRoomResult(bool won)
        {
            if (Status != RunStatus.InProgress)
            {
                return;
            }

            if (!won)
            {
                SetStatus(RunStatus.Lost);
                return;
            }

            if (IsFinalRoom)
            {
                SetStatus(RunStatus.Won);
                return;
            }

            CurrentRoomIndex++;
            RoomChanged?.Invoke(CurrentRoomNumber, TotalRooms);
        }

        private void SetStatus(RunStatus status)
        {
            if (Status == status)
            {
                return;
            }

            Status = status;
            StatusChanged?.Invoke(status);
        }
    }
}