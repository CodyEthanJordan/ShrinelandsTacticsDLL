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

        }
    }
}
