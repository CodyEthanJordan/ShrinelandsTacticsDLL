using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.BasicStructures.Events;
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
        public string Class { get; private set; }


        [JsonProperty]
        public Dictionary<StatType, Stat> Stats { get; private set; }


        [JsonProperty]
        public List<Condition> Conditions { get; private set; }
        [JsonProperty]
        public List<Action> Actions { get; private set; }
        public bool HasBeenActivated = false;
        public bool HasActed = false;
        [JsonProperty]
        public int WeaponAdvantage = 3;
        [JsonProperty]
        public int WeaponDamage = 3;
        [JsonProperty]
        public List<string> Traits = new List<string>();
        [JsonProperty]
        public List<string> Gear = new List<string>();

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
        [JsonIgnore]
        public int ArmorProtection
        {
            get
            {
                if(HasCondition("Statue"))
                {
                    var statue = Conditions.First(c => c.Name == "Statue");
                    return statue.Value;
                }
                //TODO: look for armor
                return 0;
            }
        }
        [JsonIgnore]
        public int ArmorCoverage
        {
            get
            {
                if(HasCondition("Statue"))
                {
                    var statue = Conditions.First(c => c.Name == "Statue");
                    return statue.Value;
                }
                //TODO: look for armor
                return 0;
            }
        }

        public event DungeonMaster.StatChnagedEventHandler OnStatChanged;

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

            foreach (var stat in Stats.Select(s => s.Value))
            {
                stat.OnStatChanged += StatChanged;
            }
        }

        private void StatChanged(object sender, StatChangedEventArgs a)
        {
            if(OnStatChanged != null)
            {
                StatType type = Stats.First(s => s.Value.Equals(a.NewStat)).Key;
                OnStatChanged(this, new StatChangedEventArgs(ID, type, a.NewStat));
            }
        }

        private void Stat_OnStatChanged(object sender, BasicStructures.Events.StatChangedEventArgs a)
        {
            throw new NotImplementedException();
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
            SetupEvents();
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

        public void AddTrait(string trait, GameData data)
        {
            Traits.Add(trait);
        }

        public Outcome ResolveEffect(DungeonMaster DM, Character user, Position posTarget, Deck deck, Card cardDrawn,
            List<Effect> typicalEffects)
        {
            var outcome = new Outcome();
            //TODO: add slime splitting
            if (cardDrawn.TypeOfCard == Card.CardType.Armor)
            {
                //TODO: more elegant method
                if (cardDrawn.Name == "Split")
                {
                    var emptySpaces = DM.GetEmptyAdjacentSquares(Pos);
                    if(emptySpaces.Count != 0 )
                    {
                        //int i = DungeonMaster.rand.Next(emptySpaces.Count);
                        int i = 0; //TODO: derandomized for now for networking reasons
                        var emptySpace = emptySpaces[i];
                        Vitality.Value = Vitality.Value / 2;
                        var cloneCharacter = this.Clone() as Character;
                        cloneCharacter.InitializeIndividual("Copy of " + Name, emptySpace, SideID, DM.GuidsInWaiting.Dequeue());
                        DM.CreateCharacter(cloneCharacter);
                    }
                }

                if(cardDrawn.Name == "Burn")
                {
                    if(user != null)
                    {
                        user.TakeDamage(DamageEffect.DamageType.Fire, 1);
                    }
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
            if(typeOfDamage == DamageEffect.DamageType.Fire && HasTrait("Born of Flame"))
            {
                return;
            }

            Vitality.Value -= amount; //TODO: check for 0?

            //TODO: trait system?
            if(HasTrait("Fragile Chalice"))
            {
                Mana.Value -= 1;
            }
        }

        public bool HasTrait(string trait)
        {
            return Traits.Contains(trait);
        }

        public bool HasCondition(string condition)
        {
            return Conditions.Any(c => c.Name == condition);
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

        public void InitializeIndividual(string name, Position pos, Guid SideID, Guid ID)
        {
            this.ID = ID;
            this.SideID = SideID;
            this.Name = name;
            this.Pos = pos;
            SetupEvents();
        }

        public void AddArmorCards(Deck deck, DungeonMaster DM, Character attacker, Action action)
        {
            var hit = new Card("Hit", Card.CardType.Hit);
            var armor = Card.CreateReplacementCard("Glancing Blow", Card.CardType.Armor, hit);
            deck.AddCards(armor, ArmorCoverage); //armor card causes hit to be redirected to character for resolution?

            if(HasTrait("Split"))
            {
                var split = Card.CreateReplacementCard("Split", Card.CardType.Armor, hit);
                deck.AddCards(split, 1); //TODO: magic number
            }

            if(HasTrait("Made of Flame"))
            {
                var burn = Card.CreateReplacementCard("Burn", Card.CardType.Armor, hit);
                deck.AddCards(burn, 3); //TODO: magic number
            }
        }

        internal void SetupEvents()
        {
            foreach (var stat in Stats.Select(s => s.Value))
            {
                stat.OnStatChanged += StatChanged;
            }
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
            StringBuilder sb = new StringBuilder();
            switch (verbosity)
            {
                case 0:
                    sb.Append(Name + " Vit:" + Vitality + " Sta:" + Stamina +
                        " Move:" + Move + " Pos:" + Pos);
                    if(HasBeenActivated)
                    {
                        sb.Append(" Already Activated");
                    }
                    return sb.ToString();
                case 1:
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
                    if(HasBeenActivated)
                    {
                        sb.AppendLine("Already Activated");
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

        public override string ToString()
        {
            return GetInfo(0);
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