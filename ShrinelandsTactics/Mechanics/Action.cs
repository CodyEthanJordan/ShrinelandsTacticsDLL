using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.Mechanics.Effects;
using ShrinelandsTactics.World;

namespace ShrinelandsTactics.Mechanics
{
    public class Action
    {
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public Dictionary<Character.StatType, int> Cost = new Dictionary<Character.StatType, int>();
        [JsonProperty]
        public Dictionary<CardSource, Card> DeckRecipie = new Dictionary<CardSource, Card>();

        public Action(string Name, Dictionary<Character.StatType, int> Cost,
            Dictionary<CardSource, Card> DeckRecipie)
        {
            this.Name = Name;

            this.Cost.Clear();
            foreach (var kvp in Cost)
            {
                this.Cost.Add(kvp.Key, kvp.Value);
            }

            this.DeckRecipie.Clear();
            foreach (var kvp in DeckRecipie)
            {
                this.DeckRecipie.Add(kvp.Key, kvp.Value);
            }
        }

        public void ResolveAction(DungeonMaster DM, Character user, Position posTarget,
            Character charTarget, string optionalFeatures, int? fated_roll=null)
        {
            if(!user.CanPay(this))
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
                int number = 0;

                switch (source)
                {
                    case CardSource.TargetDodge:
                        charTarget.AddDodgeCards(DM, user, deck, card);
                        break;
                    case CardSource.TargetVitality:
                        break;
                    case CardSource.TargetStamina:
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

        public enum CardSource
        {
            TargetDodge,
            TargetVitality,
            TargetStamina,
            TargetArmorCoverage,
            UserProfeciency,
        }

    }
}
