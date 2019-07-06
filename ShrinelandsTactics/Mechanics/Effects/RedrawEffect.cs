using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.World;

namespace ShrinelandsTactics.Mechanics.Effects
{
    public class RedrawEffect : Effect
    {
        [JsonProperty]
        public Dictionary<Card.CardType, List<Effect>> Effects = new Dictionary<Card.CardType, List<Effect>>();

        private RedrawEffect()
        {
            TypeOfEffect = EffectType.Redraw;
        }

        public RedrawEffect(Dictionary<Card.CardType, List<Effect>> Effects) : this()
        {
            this.Effects = Effects;
        }

        public override Outcome Apply(DungeonMaster DM, Character user, Position posTarget, 
            Character charTarget, Deck deck, Card cardDrawn, string optionalFeatures = null)
        {
            var outcome = new Outcome();
            Card card = deck.Draw();
            outcome.Message.AppendLine("Drawn:" + card.ToString());
            //inform user and target what card was drawn, possibly for temporary dodge or breaking shields
            user.CardDrawn(deck, card);
            if (charTarget != null)
            {
                charTarget.CardDrawn(deck, card); //TODO: apply to outcome and unify with Action class
            }

            //apply relevant effects
            foreach (var effect in Effects[card.TypeOfCard])
            {
                var effectOutcome = effect.Apply(DM, user, posTarget, charTarget, deck, card, "");
                outcome.Message.Append(effectOutcome.Message);
            }

            return outcome;
        }
    }
}
