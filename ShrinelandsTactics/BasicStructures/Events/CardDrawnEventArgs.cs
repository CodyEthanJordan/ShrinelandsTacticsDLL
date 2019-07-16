using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrinelandsTactics.BasicStructures.Events
{
    public class CardDrawnEventArgs : EventArgs
    {
        public Deck deck { get; private set; }
        public List<Card> cards { get; private set; }

        public CardDrawnEventArgs(Deck deck, List<Card> cards)
        {
            this.deck = deck;
            this.cards = cards;
        }
    }
}
