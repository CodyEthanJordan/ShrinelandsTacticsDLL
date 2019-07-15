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
    public class TileTests
    {
        private static GameData data;

        [ClassInitialize]
        public static void ReadGameData(TestContext context)
        {
            data = GameData.ReadDatafilesInDirectory("GameData");
        }

        
        [TestMethod]
        public void TileSerializationTest()
        {
            var tile = DebugData.GetEmptyTile();
            tile.Color = new System.Collections.Generic.List<byte>() { 0, 0, 0 };

            string json = JsonConvert.SerializeObject(tile);

            var tile2 = JsonConvert.DeserializeObject<Tile>(json);

            Assert.AreEqual(tile, tile2); //test round-trip of tile through json
        }
    }
}
