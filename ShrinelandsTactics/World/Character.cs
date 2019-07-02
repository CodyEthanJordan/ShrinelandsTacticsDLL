using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.Mechanics;

namespace ShrinelandsTactics.World
{
    public class Character : ICloneable
    {
        [JsonProperty]
        public Guid ID { get; private set; }
        [JsonProperty]
        public Guid SideID { get; private set; }
        [JsonProperty]
        public Position Pos { get; private set; }
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public Dictionary<StatType, Stat> Stats { get; private set; }
        [JsonProperty]
        public List<Condition> Conditions { get; private set; }

        [JsonIgnore]
        public Stat Vitality { get { return Stats[StatType.Vitality]; } }
        [JsonIgnore]
        public Stat Move { get { return Stats[StatType.Move]; } }
        [JsonIgnore]
        public Stat Stamina { get { return Stats[StatType.Stamina]; } }
        [JsonIgnore]
        public Stat Profeciency { get { return Stats[StatType.Profeciency]; } }
        [JsonIgnore]
        public Stat Strength { get { return Stats[StatType.Strength]; } }
        [JsonIgnore]
        public Stat Mana { get { return Stats[StatType.Mana]; } }
        
        public Character()
        {
            ID = Guid.NewGuid();
            Name = "";
            Stats = new Dictionary<StatType, Stat>();
            Conditions = new List<Condition>();
            //TODO: remove debug stuff
            Conditions.Add(new Condition("Dodging", 2));
        }

        public Character(string name, int vitality, int move, int stamina, int profeciency, int strength, int mana) : this()
        {
            this.Name = name;
            Stats.Add(StatType.Vitality, new Stat(vitality));
            Stats.Add(StatType.Move, new Stat(move));
            Stats.Add(StatType.Stamina, new Stat(stamina));
            Stats.Add(StatType.Profeciency, new Stat(profeciency));
            Stats.Add(StatType.Strength, new Stat(strength));
            Stats.Add(StatType.Mana, new Stat(mana));
        }

        public bool CanPay(Mechanics.Action action)
        {
            //TODO: factor in complicated qualities
            foreach (var kvp in action.Cost)
            {
                if(Stats[kvp.Key].Value < kvp.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public void InitializeIndividual(string name, Position pos, Guid SideID)
        {
            this.ID = Guid.NewGuid();
            this.SideID = SideID;
            this.Name = name;
            this.Pos = pos;
        }

        public void AddDodgeCards(DungeonMaster DM, Character attacker, Deck deck, Card baseCard)
        {
            //TODO: no magic numbers
            deck.AddCards(baseCard, 2);

            var dodging = Conditions.FirstOrDefault(c => c.Name == "Dodging");
            if(dodging != null)
            {
                var tempDodge = baseCard.Clone();
                tempDodge.E
            }
        }

        public enum StatType
        {
            Vitality,
            Move,
            Stamina,
            Profeciency,
            Strength,
            Mana,
        }

        public object Clone()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Character>(json);
        }
    }
}