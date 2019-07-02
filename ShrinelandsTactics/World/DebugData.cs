using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrinelandsTactics.World
{
    public static class DebugData
    {
        public static Map GetFlatlandMap(GameData data)
        {
            string description =
@"Flatland
15 20
###############
#.............#
#....#........#
#.............#
#.......#.....#
#.............#
#.#...........#
#.............#
#........#....#
#.............#
#.....###.....#
#.............#
#.............#
#.............#
#.............#
#.............#
#.............#
#.............#
#.............#
###############
";

            return Map.CreateFromText(description, data);
        }

        public static Tile GetEmptyTile()
        {
            var properties = new List<Tile.TileProperties>() { Tile.TileProperties.DebugProperty };
            return new Tile("DebugEmpty", true, 1, 'e', properties);
        }
    }
}
