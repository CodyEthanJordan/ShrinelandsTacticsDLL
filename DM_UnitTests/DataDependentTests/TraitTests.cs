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
    }
}
