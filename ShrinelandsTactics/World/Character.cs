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
        [JsonProperty]
        public int ArmorProtection = 2;
        [JsonProperty]
        public int ArmorCoverage = 2;
        [JsonProperty]
        public int WeaponAdvantage = 3;
        [JsonProperty]
        public int WeaponDamage = 3;
        [JsonProperty]
        public List<string> Traits = new List<string>();

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

        public bool Incapacitated
        {
            get { return Vitality.Value <= 0; }
        }

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

        public void NewTurn()
        {
            HasBeenActivated = false;
        }

        public void Activate()
        {
            //TODO: return an Outcome?
            Stamina.Regain(2);
            HasActed = false;
            HasBeenActivated = true;
            Move.Value = Move.Max;
            foreach (var condition in Conditions)
            {
                condition.StartingActivation();
            }
            Conditions.RemoveAll(c => c.Duration == 0);
        }

        public void EndActivation()
        {
            //usually won't mean anything
        }

        public Outcome ResolveEffect(DungeonMaster DM, Character user, Position posTarget, Deck deck, Card cardDrawn,
            List<Effect> typicalEffects)
        {
            var outcome = new Outcome();
            //TODO: add slime splitting
            if (cardDrawn.TypeOfCard == Card.CardType.Armor)
            {
                //TODO: more elegant method
                if (Traits.Contains("Split"))
                {
                    var emptySpaces = DM.GetEmptyAdjacentSquares(Pos);
                    int i = DungeonMaster.rand.Next(emptySpaces.Count);
                    var emptySpace = emptySpaces[i];
                    var cloneCharacter = this.Clone() as Character;
                    cloneCharacter.InitializeIndividual("Copy of " + Name, emptySpace, SideID);
                    DM.CreateCharacter(cloneCharacter);
                }

                //reduce damage
                var damage = typicalEffects.FirstOrDefault(e => e.TypeOfEffect == Effect.EffectType.Damage) as DamageEffect;
                if (damage != null)
                {
                    int amount = damage.GetAmount(DM, user, posTarget, this);
                    int x = Math.Max(0, amount - ArmorProtection);
                    var reducedDamage = new DamageEffect(damage.TypeOfDamage, x);
                    reducedDamage.Apply(DM, user, posTarget, this, deck, cardDrawn);
                    outcome.Message.AppendLine("Damage reduced to " + x + " by armor");
                }

                
            }

            return outcome;
        }

        public void TakeDamage(DamageEffect.DamageType typeOfDamage, int amount)
        {
            Stats[StatType.Vitality].Value -= amount; //TODO: check for 0?
        }

        public void PayCost(Action action)
        {
            foreach (var kvp in action.Cost)
            {
                Stats[kvp.Key].Value -= kvp.Value;
            }
        }

        public bool CanPay(Mechanics.Action action)
        {
            //TODO: factor in complicated qualities
            foreach (var kvp in action.Cost)
            {
                if (Stats[kvp.Key].Value < kvp.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public void CardDrawn(Deck deck, Card card)
        {
            if (card.Name == "Dodge")
            {
                ReduceCondition("Dodging", 1); //TODO: pass card to conditions and let them sort it out?
            }
            else if (card.Name == "Empowered")
            {
                ReduceCondition("Empowered", 1);
            }
            else if(card.Name == "Exertion")
            {
                ReduceCondition("Exertion", 1);
            }
        }

        public void AddCondition(string name, int amount, int duration=-1)
        {
            var condition = Conditions.FirstOrDefault(c => c.Name == name);
            if (condition == null)
            {
                condition = new Condition(name, amount, duration);
                Conditions.Add(condition); //don't have it, so gain it
                return;
            }
            condition.Value += amount;
            if (condition.Value <= 0)
            {
                Conditions.Remove(condition);
            }
        }

        public void ReduceCondition(string name, int amount)
        {
            var condition = Conditions.FirstOrDefault(c => c.Name == name);
            if (condition == null)
            {
                return; //TODO: throw error? have outcome?
            }
            condition.Value -= amount;
            if (condition.Value <= 0)
            {
                Conditions.Remove(condition);
            }
        }

        public void InitializeIndividual(string name, Position pos, Guid SideID)
        {
            this.ID = Guid.NewGuid();
            this.SideID = SideID;
            this.Name = name;
            this.Pos = pos;
        }

        public void AddArmorCards(Deck deck, DungeonMaster DM, Character attacker, Action action)
        {
            var hit = new Card("Hit", Card.CardType.Hit);
            var armor = Card.CreateReplacementCard("Glancing Blow", Card.CardType.Armor, hit);
            deck.AddCards(armor, ArmorCoverage); //armor card causes hit to be redirected to character for resolution?
        }

        public void AddDodgeCards(Deck deck, DungeonMaster DM, Character attacker, Action action)
        {
            //TODO: no magic numbers
            var defense = new Card("Defense", Card.CardType.Miss);
            deck.AddCards(defense, 2);

            //TODO: pass to condition and query
            var dodging = Conditions.FirstOrDefault(c => c.Name == "Dodging");
            if (dodging != null)
            {
                var dodge = new Card("Dodge", Card.CardType.Miss);
                deck.AddCards(dodge, dodging.Value);
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
                    foreach (var condition in Conditions)
                    {
                        sb.AppendLine(condition.ToString());
                    }
                    return sb.ToString();
                default:
                    break;
            }

            return null;
        }

        public void AddModifiers(Deck deck, DungeonMaster DM, Character user, Action action, bool isTarget)
        {
            foreach (var tag in action.Tags)
            {
                switch (tag)
                {
                    case Mechanics.Action.AbilityType.Attack:
                        if (isTarget)
                        {
                            AddDodgeCards(deck, DM, user, action);
                            AddArmorCards(deck, DM, user, action);
                            //add dodge 
                        }
                        else
                        {
                            //check for buffs to attack
                            var empowered = Conditions.FirstOrDefault(c => c.Name == "Empowered");
                            if (empowered != null)
                            {
                                deck.AddCards(new Card("Empowered", Card.CardType.Hit), empowered.Value);
                            }

                            var exertion = Conditions.FirstOrDefault(c => c.Name == "Exertion");
                            if (exertion != null)
                            {
                                deck.AddCards(new Card("Exertion", Card.CardType.Hit), exertion.Value);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        public void PayMovement(int moveCost, int staminaCost=0)
        {
            Stamina.Value -= staminaCost;
            int overFlow = Math.Max(moveCost - Move.Value, 0);
            Move.Value -= (moveCost - overFlow);
            Stamina.Value -= overFlow;
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