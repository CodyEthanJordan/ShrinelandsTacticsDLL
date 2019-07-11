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
        public Card card { get; private set; }

        public CardDrawnEventArgs(Deck deck, Card card)
        {
            this.deck = deck;
            this.card = card;
        }
    }
}
