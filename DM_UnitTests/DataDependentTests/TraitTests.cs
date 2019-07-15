using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ShrinelandsTactics;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.World;
using ShrinelandsTactics.Mechanics;
using System.Collections.Generic;
using ShrinelandsTactics.Mechanics.Effects;
using Action = ShrinelandsTactics.Mechanics.Action;

namespace DM_UnitTests.DataDependentTests
{
    [TestClass]
    public class TraitTests
    {
        private static GameData data = null;

        [ClassInitialize]
        public static void ReadGameData(TestContext context)
        {
            data = GameData.ReadDatafilesInDirectory("GameData");
        }

      
        [TestMethod]
        public void FragileChaliceTest()
        {
            var guy = DebugData.GetDebugCharacter();
            guy.AddTrait("Fragile Chalice", data);
            Assert.AreEqual(guy.Mana.Max, guy.Mana.Value);
            guy.TakeDamage(DamageEffect.DamageType.True, 1);
            Assert.IsTrue(guy.Mana.Value < guy.Mana.Max);
        }

        [TestMethod]
        public void FiredanceTest()
        {
            string testMap = @"Testmap
5 5
#####
#...#
#...#
#.**#
#####
";
            var DM = DungeonMaster.CreateFromMap(testMap, data);
            DM.Sides.Add(new Side("Test"));
            DM.currentSideID = DM.Sides[0].ID;
            var firedancer = new Character("Firedancer", 10, 10, 10, 10, 10, 10);
            var pleblord = new Character("Pleblord", 10, 10, 10, 10, 10, 10);
            firedancer.AddTrait("Firedance", data);
            firedancer.Pos = new Position(2, 2);
            pleblord.Pos = new Position(3, 2);
            DM.MoveCharacter(firedancer, Map.Direction.S);
        }
    }
}
