﻿using System;
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
        public Dictionary<CardSource, Card> DeckRecipe = new Dictionary<CardSource, Card>();
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
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<AbilityType> Tags = new List<AbilityType>();

        private int timesUsed = 0;


        public Action(string Name, Dictionary<Character.StatType, int> Cost,
            Dictionary<CardSource, Card> DeckRecipe, Dictionary<Card.CardType, List<Effect>> Effects,
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
            this.DeckRecipe = DeckRecipe;
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

            user.PayCost(this);

            //TODO: have DM do check
            Position target;
            if(posTarget != null)
            {
                target = posTarget;
            }
            else if(charTarget != null)
            {
                target = charTarget.Pos;
            }
            else
            {
                return true; //if no target always works
            }

            int dist = user.Pos.Distance(target);

            if(TypeOfRange == RangeType.Melee)
            {
                if(Map.GetAdjacent(user.Pos).Contains(charTarget.Pos))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (dist <= Range)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public Outcome ResolveAction(DungeonMaster DM, Character user, Position posTarget,
            Character charTarget, string optionalFeatures)
        {
            var outcome = new Outcome();
            if(!IsValidToDo(DM, user, posTarget, charTarget, optionalFeatures))
            {
                outcome.Message.AppendLine("Can't do that");
                return outcome;
            }

            timesUsed++; //TODO: check for no re-use

            if (Tags.Any(t => t == AbilityType.Uncontested))
            {
                foreach (var effect in Effects[Card.CardType.Hit])
                {
                    var effectOutcome = effect.Apply(DM, user, posTarget, charTarget, null, null, "");
                    outcome.Message.Append(effectOutcome.Message); //TODO: better way to combine
                }
                return outcome;
            }

            //generate outcome deck or mark as uncontested
            Deck deck = GetDeckFor(DM, user, posTarget, charTarget);
            outcome.Message.AppendLine(deck.ToString());

            //draw card
            //special drawing rules?
            Card card = deck.Draw();
            outcome.Message.AppendLine(card.ToString());

            //inform user and target what card was drawn, possibly for temporary dodge or breaking shields
            user.CardDrawn(deck, card);
            if(charTarget != null)
            {
                charTarget.CardDrawn(deck, card); //TODO: outcome effects
            }

            //apply relevant effects
            foreach (var effect in Effects[card.TypeOfCard])
            {
                var effectOutcome = effect.Apply(DM, user, posTarget, charTarget, deck, card, "");
                outcome.Message.Append(effectOutcome.Message); //TODO: better way to combine
            }

            if(TypeOfAction == ActionType.Major)
            {
                user.HasActed = true;
            }

            return outcome;
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
                    return charTarget.ArmorCoverage;
                case CardSource.UserBaseAttack:
                    return user.WeaponAdvantage + user.Profeciency.Value;
                case CardSource.UserBaseDamage:
                    return user.Strength.Value + user.WeaponDamage;
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


            foreach (var ingredient in DeckRecipe)
            {
                var source = ingredient.Key;
                int num = ResolveSource(source, DM, user, posTarget, charTarget);
                var card = ingredient.Value;
                deck.AddCards(card, num);
            }

            user.AddModifiers(deck, DM, user, this, false);

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
            Uncontested,
        }
    }
}
