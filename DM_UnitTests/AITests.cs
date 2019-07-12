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
    public class AITests
    {
        private static GameData data = null;

        [ClassInitialize]
        public static void ReadGameData(TestContext context)
        {
            data = GameData.ReadDatafilesInDirectory("GameData");
        }

      
        [TestMethod]
        public void ShortestPathTest()
        {
            string mapString = @"Debug Arena
8 8
########
#..#...#
#..#...#
#..#...#
#......#
#......#
#......#
########
";
        }

    }
}
