using ShrinelandsTactics.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrinelandsTactics
{
    public class DungeonMaster
    {
        public Map map;
        public List<Side> Sides = new List<Side>();
        public List<Character> Characters = new List<Character>();

        private GameData data;


        public DungeonMaster(GameData data)
        {
            this.data = data;
        }

        public static DungeonMaster GetDebugDM(GameData data)
        {
            var DM = new DungeonMaster(data);

            DM.map = DebugData.GetFlatlandMap(data);

            DM.Sides.Add(new Side("Heros"));
            DM.Sides.Add(new Side("The Foe"));




            return DM;
        }

    }
}
