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

        public Card Draw()
        {
            int i = rand.Next(Cards.Count);

            if(FatedDraws != null && FatedDraws.Count > 0)
            {
                var nextDraw = Cards.FirstOrDefault(c => c.Name == FatedDraws.First());
                if(nextDraw != null)
                {
                    FatedDraws.RemoveAt(0);
                    i = Cards.IndexOf(nextDraw);
                }
            }

            Card card = Cards[i];
            Cards.RemoveAt(i);
            DrawnCards.Add(card);
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var card in Cards)
            {
                sb.Append(card + ", ");
            }
            return sb.ToString();
        }

        public static readonly Random rand = new Random();

        public static List<string> FatedDraws = new List<string>();
        public static List<Card> DrawnCards = new List<Card>();

        public static void SetFate(string Fate)
        {
            SetFate(new List<string>() { Fate });
        }

        public static void SetFate(IEnumerable<string> Fate)
        {
            FatedDraws.Clear();
            FatedDraws.AddRange(Fate);
        }

        internal static void ClearTracking()
        {
            DrawnCards.Clear();
        }
    }
}
