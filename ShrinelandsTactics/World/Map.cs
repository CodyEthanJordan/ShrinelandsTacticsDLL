using ShrinelandsTactics.BasicStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrinelandsTactics.World
{
    public class Map
    {
        public string Name { get; private set; }
        public readonly int Width;
        public readonly int Height;
        private Dictionary<Position,Tile> tiles = new Dictionary<Position, Tile>();

        public Map(string name, int width, int height)
        {
            this.Name = name;
            this.Width = width;
            this.Height = height;

            //initialize tiles array
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles.Add(new Position(x, y), null);
                }
            }
        }

        public Tile GetTile(int x, int y)
        {
            return tiles[new Position(x, y)];
        }

        public static Map CreateFromText(string description, GameData data)
        {
            if(!IsValidMapDescription(description))
            {
                throw new ArgumentException("Map description is invalid\n" + description);
            }

            StringReader reader = new StringReader(description);

            string name = reader.ReadLine();
            string size = reader.ReadLine();
            int width = int.Parse(size.Split(' ')[0]);
            int height = int.Parse(size.Split(' ')[1]);

            var map = new Map(name, width, height);

            for (int y = 0; y < height; y++)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < width; x++)
                {
                    char tileDescription = line[x];
                    Tile tile = Tile.CreateFromText(tileDescription, data);
                    map.tiles[new Position(x, y)] = tile;
                }
            }

            return map;
        }

        public static bool IsValidMapDescription(string description)
        {
            return true; //TODO: implement map validation
        }
    }
}
