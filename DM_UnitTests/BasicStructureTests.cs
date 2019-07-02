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
    public class BasicStructureTests
    {
        
        [TestMethod]
        public void TestPositionEquality()
        {
            var pos1 = new Position(0, 0);
            var pos2 = new Position(1, 1);
            var pos3 = new Position(1, 1);
            Assert.AreNotEqual(pos1, pos2);
            Assert.AreEqual(pos2, pos3);
            Assert.AreEqual(pos2.GetHashCode(), pos3.GetHashCode());
            Assert.IsTrue(pos2 == pos3);
            Assert.AreEqual(pos2, pos2);

        }

    }
}
