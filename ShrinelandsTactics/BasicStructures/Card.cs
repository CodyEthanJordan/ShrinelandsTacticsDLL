using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ShrinelandsTactics.Mechanics.Effects;
using ShrinelandsTactics.World;

namespace ShrinelandsTactics.BasicStructures
{
    public class Card : ICloneable
    {
        [JsonProperty]
        public string Name { get; private set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty]
        public CardType TypeOfCard { get; private set; }
        [JsonProperty]
        public List<Effect> Effects { get; private set; }
        [JsonProperty]
        public bool ReplacingCard { get; set; }
        [JsonProperty]
        public Card CardToReplace { get; set; }

        private Card()
        {
            Effects = new List<Effect>();
            ReplacingCard = false;
        }

        public Card(string Name, CardType TypeOfCard, Effect effect) : this()
        {
            this.Name = Name;
            this.TypeOfCard = TypeOfCard;
            this.Effects.Add(effect);
        }

        public static Card CreateReplacementCard(string Name, CardType TypeOfCard, Effect effect, Card toReplace)
        {
            Card card = new Card(Name, TypeOfCard, effect);
            card.ReplacingCard = true;
            card.CardToReplace = toReplace;

            return card;
        }

        public object Clone()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Card>(json);
        }

        public enum CardType
        {
            Hit,
            Miss,
            Block,
            Armor,
        }

        public void ApplyEffects(DungeonMaster DM, Character user, Position posTarget, 
            Character charTarget, string optionalFeatures)
        {
            foreach (var effect in Effects)
            {
                effect.Apply(DM, user, posTarget, charTarget, optionalFeatures);
            }
        }
    }
}
