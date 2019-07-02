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

        public Card Draw(int? fatedRoll = null)
        {
            int i = rand.Next(Cards.Count);

            if(fatedRoll.HasValue)
            {
                i = fatedRoll.Value;
            }

            Card card = Cards[i];
            Cards.RemoveAt(i);
            return card;
        }

        public void Consolidate()
        {
            //combine all replacing cards
            while(Cards.Any(c => c.ReplacingCard))
            {
                var replacing = Cards.First(c => c.ReplacingCard);
                var toBeReplaced = Cards.FirstOrDefault(c => c.TypeOfCard == replacing.CardToReplace.TypeOfCard);
                if(toBeReplaced != null)
                {
                    replacing.ReplacingCard = false; //resolve card
                    Cards.Remove(toBeReplaced);
                }
                else
                {
                    //nothing to replace, remove card
                    Cards.Remove(replacing);
                }
            }
        }

        public static readonly Random rand = new Random();
    }
}
