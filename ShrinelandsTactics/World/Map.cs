﻿using ShrinelandsTactics.BasicStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShrinelandsTactics.World
{
    public class Map
    {
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public readonly int Width;
        [JsonProperty]
        public readonly int Height;
        [JsonProperty]
        public Dictionary<Position,Tile> tiles = new Dictionary<Position, Tile>();

        public event EventHandler<Position> OnTileChanged;

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

        public void MakeTile(DungeonMaster DM, Tile replacingTile, Position posTarget, GameData data)
        {
            var current = GetTile(posTarget);

            current.OnDestroy(DM);

            if(current.Name == "Shallow Pool" && replacingTile.Name == "Fire" ||
                replacingTile.Name == "Shallow Pool" && current.Name == "Fire") //TODO: make more elegant
            {
                var floor = data.GetTileByName("Floor");
                tiles[posTarget] = floor;
                if(OnTileChanged != null)
                {
                    OnTileChanged(this, posTarget);
                }
                return;
            }

            tiles[posTarget] = replacingTile;
            if(OnTileChanged != null)
            {
                OnTileChanged(this, posTarget);
            }
        }

        public static Map CreateFromBitmap(Bitmap bitmap, GameData data)
        {
            var pal = bitmap.Palette;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Test Map");
            sb.AppendLine(bitmap.Width + " " + bitmap.Height);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    int index = pal.Entries.Select((c, i) => new { i, c })
                         .Where(pair => pair.c.Name.Equals(color.Name, StringComparison.OrdinalIgnoreCase))
                         .Select(c => c.i).First();

                    if (index == 1)
                    {
                        sb.Append("#");
                    }
                    else
                    {
                        sb.Append(".");
                    }
                }
                sb.AppendLine();
            }

            Map map = Map.CreateFromText(sb.ToString(), data);

            return map;
        }

        public static Map CreateFromCharArray(List<List<char>> tiles, GameData data)
        {
            throw new NotImplementedException();
        }

        public List<Tile> GetAdjacentTiles(Position pos)
        {
            var tiles = new List<Tile>();
            foreach (var dir in DirectionToPosition)
            {
                tiles.Add(GetTile(pos + dir.Value));
            }

            return tiles;
        }

        public Tile GetTile(Position pos)
        {
            return tiles[pos];
        }

        public Tile GetTile(int x, int y)
        {
            return tiles[new Position(x, y)];
        }

        public bool IsPassable(Position pos)
        {
            return tiles[pos].Passable;
        }

        public static List<Position> GetAdjacent(Position pos)
        {
            var positions = new List<Position>();
            foreach (var dir in Enum.GetValues(typeof(Direction)))
            {
                var offset = DirectionToPosition[(Direction)dir];
                positions.Add(pos + offset);
            }
            return positions;
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Name);
            sb.AppendLine(Width + " " + Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    sb.Append(GetTile(x, y).ToString());
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static Direction ParseDirection(string dir)
        {
            return (Direction)Enum.Parse(typeof(Direction), dir.ToUpper());
        }

        public static bool IsValidMapDescription(string description)
        {
            return true; //TODO: implement map validation
        }

        public enum Direction
        {
            N, S, E, W, NE, NW, SW, SE
        }

        public static readonly Dictionary<Direction, Position> DirectionToPosition =
            new Dictionary<Direction, Position>()
            {
                { Direction.N, new Position(0, -1) },
                { Direction.S, new Position(0, 1) },
                { Direction.E, new Position(1, 0) },
                { Direction.W, new Position(-1, 0) },
                { Direction.NW, new Position(-1, -1) },
                { Direction.SW, new Position(-1, 1) },
                { Direction.NE, new Position(1, -1) },
                { Direction.SE, new Position(1, 1) },
            };

        public void NewTurn()
        {
            //TODO: update tiles
        }

        public int GetGamestateHash()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ Name.GetHashCode();
                hash = (hash * 16777619) ^ Width;
                hash = (hash * 16777619) ^ Height;
                foreach (var kvp in tiles)
                {
                    hash = (hash * 16777619) ^ kvp.Key.GetHashCode();
                    hash = (hash * 16777619) ^ kvp.Value.GetGamestateHash();
                }
                return hash;
            }
        }
    }
}
