using Newtonsoft.Json;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.Mechanics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrinelandsTactics.World
{
    public class Tile : ICloneable
    {
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public bool Passable { get; private set; }
        [JsonProperty]
        public int MoveCost { get; private set; }
        [JsonProperty]
        public char Icon { get; private set; }
        [JsonProperty]
        public readonly List<TileProperties> Properties = new List<TileProperties>();

        //TODO: add OnEnter and OnExit methods, as well as OnTurn
        public Tile()
        {

        }

        public Tile(string Name, bool Passable, int MoveCost, char Icon, List<TileProperties> Properties)
        {
            this.Name = Name;
            this.Passable = Passable;
            this.MoveCost = MoveCost;
            this.Icon = Icon;
            this.Properties.Clear();
            if(Properties != null)
            {
                this.Properties.AddRange(Properties);
            }
        }

        public object Clone()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Tile>(json);
        }

        public enum TileProperties
        {
            OnFire,
            Liquid,
            DebugProperty,
        }

        public override bool Equals(object obj)
        {
            var other = obj as Tile;
            if (other == null)
            {
                return false;
            }
            return this.Name == other.Name && 
                this.Passable == other.Passable &&
                this.MoveCost == other.MoveCost &&
                this.Properties.All(other.Properties.Contains) && this.Properties.Count == other.Properties.Count;
        }

        public static Tile CreateFromText(char tileIcon, GameData data)
        {
            var tile = data.Tiles.Values.FirstOrDefault(t => t.Icon == tileIcon);

            if(tile == null)
            {
                throw new ArgumentException("No such tile as " + tileIcon);
            }

            return tile.Clone() as Tile;
        }

        internal void AddSituationalModifiers(Deck deck, Mechanics.Action action, Character user, Position posTarget, Character charTarget, bool v)
        {
            return; //TODO: pass to files
            throw new NotImplementedException();
        }
    }
}
