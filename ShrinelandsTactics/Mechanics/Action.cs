using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.Mechanics.Effects;
using ShrinelandsTactics.World;

namespace ShrinelandsTactics.Mechanics
{
    public class Action : ICloneable
    {
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public Dictionary<Character.StatType, int> Cost = new Dictionary<Character.StatType, int>();
        [JsonProperty]
        public Dictionary<CardSource, Card> DeckRecipie = new Dictionary<CardSource, Card>();
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty]
        public RangeType TypeOfRange;
        [JsonProperty]
        public int Range;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty]
        public ActionType TypeOfAction = ActionType.Major;
        [JsonProperty]
        public bool Repeatable = false;

        private int timesUsed = 0;


        public Action(string Name, Dictionary<Character.StatType, int> Cost,
            Dictionary<CardSource, Card> DeckRecipie, RangeType TypeOfRange, int Range,
            ActionType TypeOfAction = ActionType.Major, bool Repeatable = false)
        {
            this.Name = Name;
            this.TypeOfRange = TypeOfRange;
            this.Range = Range;
            this.TypeOfAction = TypeOfAction;
            this.Repeatable = Repeatable;

            this.Cost = Cost;
            this.DeckRecipie = DeckRecipie;
        }

        public bool IsValidToDo(DungeonMaster DM, Character user, Position posTarget,
            Character charTarget, string optionalFeatures)
        {
            if(!user.CanPay(this))
            {
                return false;
            }

            Position target;
            if(posTarget != null)
            {
                target = posTarget;
            }
            else
            {
                target = charTarget.Pos;
            }

            int dist = user.Pos.Distance(target);

            if(dist <= Range)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ResolveAction(DungeonMaster DM, Character user, Position posTarget,
            Character charTarget, string optionalFeatures, int? fated_roll=null)
        {
            if(!IsValidToDo(DM, user, posTarget, charTarget, optionalFeatures))
            {
                return; //TODO: generate error or raise event?
            }

            //generate outcome deck or mark as uncontested
            Deck deck = GetDeckFor(DM, user, posTarget, charTarget);

            //draw card
            //special drawing rules?
            Card card = deck.Draw(fated_roll);

            //TODO: inform user and target what card was drawn, possibly for temporary dodge or breaking shields

            //apply relevant effects
            card.ApplyEffects(DM, user, posTarget, charTarget, optionalFeatures);
        }

        public Deck GetDeckFor(DungeonMaster DM, Character user, Position posTarget,
            Character charTarget)
        {
            Deck deck = new Deck();

            foreach (var ingredient in DeckRecipie)
            {
                var source = ingredient.Key;
                var card = ingredient.Value;

                switch (source)
                {
                    case CardSource.TargetDodge:
                        charTarget.AddDodgeCards(DM, user, deck, card);
                        break;
                    case CardSource.TargetVitality:
                        deck.AddCards(card, charTarget.Vitality.Value);
                        break;
                    case CardSource.TargetStamina:
                        deck.AddCards(card, charTarget.Stamina.Value);
                        break;
                    case CardSource.TargetArmorCoverage:
                        charTarget.AddArmorCards(DM, user, deck, card);
                        break;
                    case CardSource.UserProfeciency:
                        deck.AddCards(card, user.Profeciency.Value);
                        //TODO: maybe pass effect and cardsource to character and have it figure the number
                        break;
                    default:
                        break;
                }
            }

            deck.Consolidate();
            return deck;
        }

        public object Clone()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Action>(json);
        }

        public enum CardSource
        {
            TargetDodge,
            TargetVitality,
            TargetStamina,
            TargetArmorCoverage,
            UserProfeciency,
        }

        public enum RangeType
        {
            Melee,
            Ranged,
        }

        public enum ActionType
        {
            Major,
            Minor,
        }
    }
}
