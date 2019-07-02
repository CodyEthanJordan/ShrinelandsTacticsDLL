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
        public void GameDataTileDeserializationTest()
        {
            Assert.IsTrue(data.Tiles.Count > 0);
            Assert.IsTrue(data.Tiles.Values.Any(t => t.Name == "DebugWall"));
            var debugWall = data.Tiles["DebugWall"];

            Assert.AreEqual(1, debugWall.MoveCost);
            Assert.AreEqual(false, debugWall.Passable);
            Assert.IsTrue(debugWall.Properties.Count == 1);
            Assert.IsTrue(debugWall.Properties.Contains(Tile.TileProperties.DebugProperty));
            Assert.AreEqual('D', debugWall.Icon);
        }

        [TestMethod]
        public void TileSerializationTest()
        {
            var tile = DebugData.GetEmptyTile();

            string json = JsonConvert.SerializeObject(tile);

            var tile2 = JsonConvert.DeserializeObject<Tile>(json);

            Assert.AreEqual(tile, tile2); //test round-trip of tile through json
        }
    }
}
