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

namespace DM_UnitTests
{
    [TestClass]
    public class ActionEffectTests
    {
        private static GameData data = null;

        [ClassInitialize]
        public static void ReadGameData(TestContext context)
        {
            data = GameData.ReadDatafilesInDirectory("GameData");
        }

        [TestMethod]
        public void CanPayTest()
        {
            var guy = DebugData.GetDebugCharacter();
            var cost = new Dictionary<Character.StatType, int>() { { Character.StatType.Stamina,1} };
            var effects = new Dictionary<Card, Effect>();
            var recipie = new Dictionary<Action.CardSource, Card>();
            var cheapAction = new Action("test", cost, effects, recipie);
            cost[Character.StatType.Stamina] = 9999;
            var expensiveAction = new Action("test", cost, effects, recipie);

            Assert.IsTrue(guy.CanPay(cheapAction));
            Assert.IsFalse(guy.CanPay(expensiveAction));
        }
    }
}
