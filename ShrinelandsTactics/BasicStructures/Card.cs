using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShrinelandsTactics.BasicStructures
{
    public class Card : ICloneable
    {
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public CardType TypeOfCard { get; private set; }

        public Card(string Name, CardType TypeOfCard)
        {
            this.Name = Name;
            this.TypeOfCard = TypeOfCard;
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
    }
}
