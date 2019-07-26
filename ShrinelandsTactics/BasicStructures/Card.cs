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
        public bool ReplacingCard { get; set; }
        [JsonProperty]
        public Card CardToReplace { get; set; }

        private Card()
        {
            ReplacingCard = false;
        }

        public Card(string Name, CardType TypeOfCard) : this()
        {
            this.Name = Name;
            this.TypeOfCard = TypeOfCard;
        }

        public static Card CreateReplacementCard(string Name, CardType TypeOfCard, Card toReplace)
        {
            Card card = new Card(Name, TypeOfCard);
            card.ReplacingCard = true;
            card.CardToReplace = toReplace;

            return card;
        }

        public override string ToString()
        {
            return Name + " (" + TypeOfCard + ")";
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
            Encounter,
        }
    }
}
