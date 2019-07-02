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
        public DungeonMaster(GameData data)
        {
            map = DebugData.GetFlatlandMap(data);
        }

    }
}
