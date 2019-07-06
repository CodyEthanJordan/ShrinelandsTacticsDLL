using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ShrinelandsTactics;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.World;

namespace DM_UnitTests
{
    [TestClass]
    public class MapTests
    {
        private static GameData data;

        [ClassInitialize]
        public static void ReadGameData(TestContext context)
        {
            data = GameData.ReadDatafilesInDirectory("GameData");
        }

        [TestMethod]
        public void MapLoadingTest()
        {
            Bitmap bitmap = new Bitmap("GameData/ExampleCaveMap.gif");

            Map map = Map.CreateFromBitmap(bitmap, data);

            Assert.IsNotNull(map);
            Assert.IsTrue(map.GetTile(14, 18).Passable == false);
            Assert.IsTrue(map.GetTile(18, 14).Passable == true);
        }
       
    }
}
