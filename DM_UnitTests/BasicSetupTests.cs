using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ShrinelandsTactics;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.World;

namespace DM_UnitTests
{
    [TestClass]
    public class BasicSetupTests
    {
        private static GameData data;

        [ClassInitialize]
        public static void ReadGameData(TestContext context)
        {
            data = GameData.ReadDatafilesInDirectory("GameData");
        }

        [TestMethod]
        public void StatSerializationTest()
        {
            var stat1 = new Stat(3, 7);
            string json = JsonConvert.SerializeObject(stat1);
            var stat2 = JsonConvert.DeserializeObject<Stat>(json);
            Assert.AreEqual(stat1.Value, stat2.Value);
            Assert.AreEqual(stat1.Max, stat2.Max);
        }

        [TestMethod]
        public void DebugMapTest()
        {
            var map = DebugData.GetFlatlandMap(data);
            Assert.AreEqual("Flatland", map.Name);
            Assert.AreEqual(15, map.Width);
            Assert.AreEqual(20, map.Height);

            //test a a few tiles at "random"
            var floorTile = map.GetTile(1, 1);
            Assert.AreEqual(true, floorTile.Passable);
            var wallTile = map.GetTile(0, 0);
            Assert.AreEqual(false, wallTile.Passable);
        }


    }
}
