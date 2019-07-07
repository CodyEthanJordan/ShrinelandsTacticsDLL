using System;
using System.Collections.Generic;
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

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Test Map");
            sb.AppendLine(bitmap.Width + " " + bitmap.Height);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Height; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    sb.Append(data.GetIconByColor(color.R, color.G, color.B).ToString());
                }
                sb.AppendLine();
            }

            Map map = Map.CreateFromText(sb.ToString(), data);

            Assert.IsNotNull(map);
            Assert.IsTrue(map.GetTile(14, 18).Passable == false);
            Assert.IsTrue(map.GetTile(18, 14).Passable == true);
        }
       
    }
}
