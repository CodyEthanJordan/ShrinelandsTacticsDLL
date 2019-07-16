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
        public List<byte> Color = new List<byte>();
        [JsonProperty]
        public readonly List<TileProperties> Properties = new List<TileProperties>();
        [JsonProperty]
        public Position Target = null;
        [JsonProperty]
        public string Description;

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

        public override string ToString()
        {
            return Icon.ToString();
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

        public void AddSituationalModifiers(Deck deck, Mechanics.Action action, Character user, Position posTarget, Character charTarget, bool v)
        {
            return; //TODO: pass to files
            throw new NotImplementedException();
        }

        public int GetGamestateHash()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ Name.GetHashCode();
                hash = (hash * 16777619) ^ Passable.GetHashCode();
                hash = (hash * 16777619) ^ MoveCost.GetHashCode();
                foreach (var prop in Properties)
                {
                    hash = (hash * 16777619) ^ prop.GetHashCode();
                }
                return hash;
            }
        }

        public void OnDestroy(DungeonMaster DM)
        {
            //TODO: remove teleportal

        }

        public void CharacterEntered(DungeonMaster DM, Character guy)
        {
            if(Name == "Entrance" && guy.HasCondition("Treasure"))
            {
                guy.ReduceCondition("Treasure", 1);
                DM.ScorePoint(guy.SideID, 1);
            }

            if(Properties.Contains(TileProperties.Teleportal) && Target != null)
            {
                if(guy.HasCondition("Teleported"))
                {
                    guy.ReduceCondition("Teleported", 1);
                }
                else
                {
                    guy.AddCondition("Teleported", 1);
                    //telefrag nerds
                    var nerd = DM.Characters.FirstOrDefault(c => c.Pos == Target);
                    if(nerd != null)
                    {
                        nerd.TakeDamage(Mechanics.Effects.DamageEffect.DamageType.True, 999);
                    }
                    DM.TeleportTo(guy, Target);
                }
            }

            if(Properties.Contains(TileProperties.OnFire) && !guy.HasTrait("Firedance"))
            {
                guy.TakeDamage(Mechanics.Effects.DamageEffect.DamageType.Fire, 1);
            }

            //TODO: use properties?
            if(Name == "Shallow Pool" && guy.HasTrait("Weakness to Water"))
            {
                guy.TakeDamage(Mechanics.Effects.DamageEffect.DamageType.True, 5); //TODO: magic number
            }

            if(Properties.Contains(TileProperties.Ooze) && !guy.HasTrait("One with Filth"))
            {
                guy.TakeDamage(Mechanics.Effects.DamageEffect.DamageType.Magic, 1);
                guy.Stamina.Value -= 1;
            }

            if(Properties.Contains(TileProperties.Treasure) && !guy.HasTrait("Mindless") && 
                guy.Stamina.Value > 0 && !guy.HasCondition("Treasure"))
            {
                guy.Stamina.Value -= 1;
                guy.AddCondition("Treasure", 1);
                var floor = DM.data.GetTileByName("Floor");
                DM.map.MakeTile(DM, floor, guy.Pos, DM.data);
            }
        }

        internal void CharacterActivated(DungeonMaster dungeonMaster, Character guy)
        {
            if(guy.HasTrait("Gather Power") && Properties.Contains(TileProperties.OnFire))
            {
                guy.Mana.Regain(1); //TODO: rename gather power?
            }

            if(guy.HasTrait("Oozeglide") && Properties.Contains(TileProperties.Ooze))
            {
                guy.Move.Value += 2; //TODO: magic number
            }
        }

        public int MoveCostFor(Character guy)
        {
            if(guy.HasTrait("Flying"))
            {
                return 1;
            }

            if(Properties.Contains(TileProperties.Ooze))
            {
                if (guy.HasTrait("Oozeglide"))
                {
                    return 0;
                }
                else if (guy.HasTrait("One with Filth"))
                {
                    return 1;
                }
            }

            return MoveCost;
        }

        public void CharacterDeactivated(Character guy)
        {
            if(Properties.Contains(TileProperties.OnFire))
            {
                guy.TakeDamage(Mechanics.Effects.DamageEffect.DamageType.Fire, 3); //TODO: magic number
            }
        }

        public enum TileProperties
        {
            OnFire,
            Liquid,
            DebugProperty,
            Treasure,
            Ooze,
            Teleportal
        }
    }
}
