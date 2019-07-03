using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.Mechanics;
using ShrinelandsTactics.Mechanics.Effects;
using Action = ShrinelandsTactics.Mechanics.Action;

namespace ShrinelandsTactics.World
{
    public class Character : ICloneable
    {
        [JsonProperty]
        public Guid ID { get; private set; }
        [JsonProperty]
        public Guid SideID { get; private set; }
        [JsonProperty]
        public Position Pos { get; set; } //TODO: maybe should be private, use method?
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public Dictionary<StatType, Stat> Stats { get; private set; }
        [JsonProperty]
        public List<Condition> Conditions { get; private set; }
        [JsonProperty]
        public List<Action> Actions { get; private set; }
        public bool HasBeenActivated = false;
        public bool HasActed = false;

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
            Actions = new List<Mechanics.Action>();
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

        internal void TakeDamage(DamageEffect.DamageType typeOfDamage, int amount)
        {
            Stats[StatType.Vitality].Value -= amount; //TODO: check for 0?
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

        public void AddArmorCards(DungeonMaster DM, Character attacker, Deck deck, Card card)
        {
            //reduce damage by 2 TODO: no hardcode
            //TODO: does null break this?
            DamageEffect damage = card.Effects.FirstOrDefault(e => e.TypeOfEffect == Effect.EffectType.Damage) as DamageEffect;
            if(damage != null)
            {
                damage.Amount -= 2; //TODO: don't go negative
            }

            deck.AddCards(card, 2); //TODO: check actual items

        }

        public void AddDodgeCards(DungeonMaster DM, Character attacker, Deck deck, Card baseCard)
        {
            //TODO: no magic numbers
            deck.AddCards(baseCard, 2);

            var dodging = Conditions.FirstOrDefault(c => c.Name == "Dodging");
            if(dodging != null)
            {
                var tempDodge = baseCard.Clone() as Card;
                var reduceDodge = new ModifyConditionEffect("Dodging", -1, this);
                tempDodge.Effects.Add(reduceDodge);
                deck.AddCards(tempDodge, dodging.Value);
            }
        }

        public string GetInfo(int verbosity)
        {
            switch (verbosity)
            {
                case 0:
                    return Name + " Vit:" + Vitality + " Sta:" + Stamina + 
                        " Move:" + Move + " Pos:" + Pos;
                case 1:
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(Name);
                    foreach (var kvp in Stats)
                    {
                        sb.AppendLine(kvp.Key + ":" + kvp.Value);
                    }
                    for (int i = 0; i < Actions.Count; i++)
                    {
                        sb.Append(i + ":" + Actions[i].Name + "  ");
                    }
                    sb.AppendLine();
                    return sb.ToString();
                default:
                    break;
            }

            return null;
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