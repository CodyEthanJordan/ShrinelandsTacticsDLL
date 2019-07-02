using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShrinelandsTactics.BasicStructures
{
    public class Deck
    {

        public List<Card> Cards = new List<Card>();

        public void AddCards(Card card, int number)
        {
            for (int i = 0; i < number; i++)
            {
                Cards.Add(card.Clone() as Card);
            }
        }
    }
}
