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
    public class FancyAbilityTests
    {
        private static GameData data = null;

        [ClassInitialize]
        public static void ReadGameData(TestContext context)
        {
            data = GameData.ReadDatafilesInDirectory("GameData");
        }

      
        [TestMethod]
        public void BecomeStoneTest()
        {
            var guy = DebugData.GetDebugCharacter();
            var becomeStone = data.GiveAction("Become Stone", guy);
            Outcome outcome = new Outcome();
            becomeStone.ResolveAction(null, guy, null, null, null, outcome);
            Assert.IsTrue(guy.ArmorCoverage > 5);
        }

        [TestMethod]
        public void PourOutTest()
        {
            string testMap = @"Testmap
5 5
#####
#...#
#~.##
#.*.#
#####
";

            string encounterYaml = @"encounter_name: Test

sides:
 - Heros
 - The Foe

characters:
 - name: Zach
   class: Knight
   pos: 13 25
   side: Heros";

            var DM = DungeonMaster.CreateFromMap(testMap, data);
            DM.Sides.Add(new Side("Test"));
            DM.currentSideID = DM.Sides[0].ID;
            var chaliceBearer = new Character("Chalice Bearer", 10, 10, 10, 10, 10, 10);
            var injuredGuy = new Character("Injured", 10, 10, 10, 10, 10, 10);
            injuredGuy.TakeDamage(DamageEffect.DamageType.True, 1);
            chaliceBearer.Pos = new Position(2, 2);
            injuredGuy.Pos = new Position(2, 1);
            DM.CreateCharacter(chaliceBearer);
            DM.CreateCharacter(injuredGuy);
            string vis = DM.VisualizeWorld();

            var pourOut = data.Actions.First(a => a.Name == "Pour Out");
            var validTargets = pourOut.GetValidTargets(DM, chaliceBearer);
            Assert.AreEqual(7, validTargets.Count);

            var outcome = new Outcome();
            pourOut.ResolveAction(DM, chaliceBearer, chaliceBearer.Pos + Map.DirectionToPosition[Map.Direction.N], injuredGuy, "", outcome);
            Assert.AreEqual(injuredGuy.Vitality.Max, injuredGuy.Vitality.Value);
            Assert.AreEqual("Shallow Pool", DM.map.GetTile(injuredGuy.Pos).Name);

            var south = chaliceBearer.Pos + Map.DirectionToPosition[Map.Direction.S];
            pourOut.ResolveAction(DM, chaliceBearer, south, null, "", outcome);
            Assert.AreEqual("Floor", DM.map.GetTile(south).Name);
        }
    }
}
