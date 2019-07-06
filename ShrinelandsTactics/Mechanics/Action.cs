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
        [JsonProperty]
        public Dictionary<Card.CardType, List<Effect>> Effects = new Dictionary<Card.CardType, List<Effect>>();
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
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty]
        public List<AbilityType> Tags = new List<AbilityType>();

        private int timesUsed = 0;


        public Action(string Name, Dictionary<Character.StatType, int> Cost,
            Dictionary<CardSource, Card> DeckRecipie, Dictionary<Card.CardType, List<Effect>> Effects,
            RangeType TypeOfRange, int Range, 
            ActionType TypeOfAction = ActionType.Major, bool Repeatable = false)
        {
            this.Name = Name;
            this.TypeOfRange = TypeOfRange;
            this.Range = Range;
            this.TypeOfAction = TypeOfAction;
            this.Repeatable = Repeatable;
            this.Effects = Effects;

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

            if(TypeOfAction == ActionType.Major && user.HasActed)
            {
                return false; //already taken major action
            }

            //TODO: have DM do check
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
            Character charTarget, string optionalFeatures)
        {
            if(!IsValidToDo(DM, user, posTarget, charTarget, optionalFeatures))
            {
                return; //TODO: generate error or raise event?
            }

            timesUsed++; //TODO: check for no re-use

            //generate outcome deck or mark as uncontested
            Deck deck = GetDeckFor(DM, user, posTarget, charTarget);

            //draw card
            //special drawing rules?
            Card card = deck.Draw();

            //inform user and target what card was drawn, possibly for temporary dodge or breaking shields
            user.CardDrawn(deck, card);
            if(charTarget != null)
            {
                charTarget.CardDrawn(deck, card);
            }

            //apply relevant effects
            foreach (var effect in Effects[card.TypeOfCard])
            {
                effect.Apply(DM, user, posTarget, charTarget, deck, card, "");
            }
        }

        public static int ResolveSource(CardSource source, DungeonMaster DM, Character user, 
            Position posTarget, Character charTarget)
        {
            switch (source)
            {
                case CardSource.TargetVitality:
                    return charTarget.Vitality.Value;
                case CardSource.TargetStamina:
                    return charTarget.Vitality.Value;
                case CardSource.TargetArmorCoverage:
                    return charTarget.armorCoverage;
                case CardSource.UserBaseAttack:
                    return user.weaponAdvantage + user.Profeciency.Value;
                case CardSource.UserBaseDamage:
                    return user.Strength.Value + user.weaponDamage;
                case CardSource.UserStrength:
                    return user.Strength.Value;
                default:
                    break;
            }
            throw new NotImplementedException();
        }

        public Deck GetDeckFor(DungeonMaster DM, Character user, Position posTarget,
            Character charTarget)
        {
            Deck deck = new Deck();

            foreach (var ingredient in DeckRecipie)
            {
                var source = ingredient.Key;
                int num = ResolveSource(source, DM, user, posTarget, charTarget);
                var card = ingredient.Value;
                deck.AddCards(card, num);
            }

            if(charTarget != null)
            {
                charTarget.AddModifiers(deck, DM, user, this, true);
            }
            DM.AddSituationalModifiers(deck, this, user, posTarget, charTarget);

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
            TargetVitality,
            TargetStamina,
            TargetArmorCoverage,
            UserBaseAttack,
            UserStrength,
            UserBaseDamage,
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

        public enum AbilityType
        {
            Attack,
        }
    }
}
