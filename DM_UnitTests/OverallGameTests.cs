using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ShrinelandsTactics;
using ShrinelandsTactics.World;

namespace DM_UnitTests
{
    [TestClass]
    public class OverallGameTests
    {
        private static GameData data;

        [ClassInitialize]
        public static void ReadGameData(TestContext context)
        {
            data = GameData.ReadDatafilesInDirectory("GameData");
        }
       
        [TestMethod]
        public void MoveRobbyAround()
        {
            var DM = DungeonMaster.GetDebugDM(data);
            var robby = DM.Characters[0];
            var zach = DM.Characters[1];

            var startingPos = robby.Pos;
            var outcome = DM.MoveCharacter(robby, Map.Direction.S);
            Assert.IsNotNull(outcome);
            Assert.AreEqual(startingPos, robby.Pos); //shouldn't have moved since he isn't activated yet

            outcome = DM.Activate(robby);
            outcome = DM.MoveCharacter(robby, Map.Direction.S);
            Assert.AreNotEqual(startingPos, robby.Pos);

            startingPos = zach.Pos;
            DM.MoveCharacter(zach, Map.Direction.S);
            Assert.AreEqual(startingPos, zach.Pos); //shouldn't move since its not his side's turn
        }

        [TestMethod]
        public void TurnPassingTest()
        {
            var DM = DungeonMaster.GetDebugDM(data);
            var robby = DM.Characters[0];
            var zach = DM.Characters[1];

            Assert.IsTrue(DM.currentSide.ID == robby.SideID);

            DM.Activate(robby);
            DM.MoveCharacter(robby, Map.Direction.S);
            robby.HasActed = true;
            DM.EndTurn();

            Assert.IsTrue(DM.currentSide.ID == zach.SideID);
            Assert.AreEqual(0, DM.TurnCount);
            Assert.IsFalse(zach.HasBeenActivated);
            Assert.IsFalse(zach.HasActed);
            Assert.IsTrue(robby.HasActed);

            DM.EndTurn();

            Assert.IsTrue(zach.HasBeenActivated);
            Assert.IsTrue(robby.HasActed);
            Assert.IsFalse(robby.HasBeenActivated);
            Assert.IsTrue(robby.Move.Value < robby.Move.Max);
            Assert.AreEqual(1, DM.TurnCount);

            DM.Activate(robby);

            Assert.IsTrue(robby.HasBeenActivated);
            Assert.AreEqual(robby.Move.Max, robby.Move.Value);
           
        }
    }
}
