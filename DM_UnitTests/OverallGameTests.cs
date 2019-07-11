using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ShrinelandsTactics;
using ShrinelandsTactics.BasicStructures.Events;
using ShrinelandsTactics.World;
using YamlDotNet;
using YamlDotNet.RepresentationModel;

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

            bool eventRaised = false;
            DM.OnCharacterMoved += delegate (object sender, CharacterMovedEventArgs e)
            {
                Assert.AreEqual("Robby", e.Name);
                eventRaised = true;
            };
            outcome = DM.Activate(robby);
            outcome = DM.MoveCharacter(robby, Map.Direction.S);
            Assert.IsTrue(eventRaised);
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
            Assert.AreEqual(robby.SideID, DM.currentSide.ID);
            Assert.AreEqual(1, DM.TurnCount);

            DM.Activate(robby);

            Assert.IsTrue(robby.HasBeenActivated);
            Assert.AreEqual(robby.Move.Max, robby.Move.Value);
        }

        [TestMethod]
        public void DMSynrchronizationTest()
        {
            var DM1 = DungeonMaster.GetDebugDM(data);
            var DM2 = DM1.Clone() as DungeonMaster;
            DM2.data = DM1.data;

            Assert.AreEqual(DM1.map.GetGamestateHash(), DM2.map.GetGamestateHash());
            Assert.AreEqual(DM1.Sides[0].ID.GetHashCode(), DM2.Sides[0].ID.GetHashCode());
            Assert.AreEqual(DM1.Sides[0].Name.GetHashCode(), DM2.Sides[0].Name.GetHashCode());
            Assert.AreEqual(DM1.GetGamestateHash(), DM2.GetGamestateHash());

            var robby = DM1.Characters.FirstOrDefault(c => c.Name == "Robby");

            var activateOutcome = DM1.Activate(robby);
            DM2.ApplyOutcome(activateOutcome);
            Assert.AreEqual(DM1.GetGamestateHash(), DM2.GetGamestateHash());

            var moveOutcome = DM1.MoveCharacter(robby, Map.Direction.S);
            DM2.ApplyOutcome(moveOutcome);
            Assert.AreEqual(DM1.GetGamestateHash(), DM2.GetGamestateHash());

            var attackOutcome = DM1.UseAbility("Attack", new List<string>() { "Zach" });
            DM2.ApplyOutcome(attackOutcome);
            Assert.AreEqual(DM1.GetGamestateHash(), DM2.GetGamestateHash());
        }
    }
}
