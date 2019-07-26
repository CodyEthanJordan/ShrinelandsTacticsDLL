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

namespace DM_UnitTests.Travel
{
    [TestClass]
    public class EncounterTests
    {
      
        [TestMethod]
        public void WolfEncounterTest()
        {
            var TM = new TravelMaster();
            var wolves = DebugData.GetMistWolfEncounter();
            TM.ChooseOption(wolves, 0);
        }
    }
}
