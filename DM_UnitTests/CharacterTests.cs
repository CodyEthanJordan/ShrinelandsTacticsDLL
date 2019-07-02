using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ShrinelandsTactics;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.World;

namespace DM_UnitTests
{
    [TestClass]
    public class CharcterTests
    {
        private static GameData data = null;

        [ClassInitialize]
        public static void ReadGameData(TestContext context)
        {
            data = GameData.ReadDatafilesInDirectory("GameData");
        }

        [TestMethod]
        public void TestDebugJsonCharacter()
        {
            var debugGuy = data.Characters.FirstOrDefault(c => c.Name == "Debug Guy");
            Assert.AreEqual(10, debugGuy.Vitality.Value);
        }
      

        [TestMethod]
        public void RoundTripSerializationTest()
        {
            var char1 = DebugData.GetDebugCharacter();

            string json = JsonConvert.SerializeObject(char1);

            var char2 = JsonConvert.DeserializeObject<Character>(json);

            Assert.AreEqual(char1.Name, char2.Name);
            Assert.AreEqual(char1.Vitality.Value, char2.Vitality.Value);
        }
    }
}
