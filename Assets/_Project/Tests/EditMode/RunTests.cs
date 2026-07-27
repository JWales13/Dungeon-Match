using System.Collections.Generic;
using NUnit.Framework;
using Game.Core;

namespace Game.Tests
{
    /// <summary>
    /// Run progression rules, verified with no Unity and no boards - a run is
    /// driven purely by win/lose booleans, so every path is deterministic.
    /// </summary>
    public class RunTests
    {
        private static List<RoomDefinition> Rooms(int count)
        {
            var rooms = new List<RoomDefinition>();
            for (int i = 0; i < count; i++)
            {
                rooms.Add(new RoomDefinition(monsterHealth: 10, moveLimit: 10, damagePerTile: 1));
            }

            return rooms;
        }

        [Test]
        public void WinningTheOnlyRoom_WinsTheRun()
        {
            var run = new Run(Rooms(1));

            run.RegisterRoomResult(won: true);

            Assert.AreEqual(RunStatus.Won, run.Status);
        }

        [Test]
        public void WinningANonFinalRoom_AdvancesButDoesNotEndTheRun()
        {
            var run = new Run(Rooms(3));

            run.RegisterRoomResult(won: true);

            Assert.AreEqual(RunStatus.InProgress, run.Status);
            Assert.AreEqual(2, run.CurrentRoomNumber);
        }

        [Test]
        public void ClearingEveryRoom_WinsTheRun()
        {
            var run = new Run(Rooms(3));

            run.RegisterRoomResult(won: true);
            run.RegisterRoomResult(won: true);
            run.RegisterRoomResult(won: true);

            Assert.AreEqual(RunStatus.Won, run.Status);
        }

        [Test]
        public void LosingAnyRoom_EndsTheRunImmediately()
        {
            var run = new Run(Rooms(3));

            run.RegisterRoomResult(won: true);
            run.RegisterRoomResult(won: false);

            Assert.AreEqual(RunStatus.Lost, run.Status);
        }

        [Test]
        public void ResultsAfterTheRunEnds_AreIgnored()
        {
            var run = new Run(Rooms(2));

            run.RegisterRoomResult(won: false); // lost
            run.RegisterRoomResult(won: true);  // must be a no-op

            Assert.AreEqual(RunStatus.Lost, run.Status);
        }
    }
}