using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrinelandsTactics.BasicStructures.Events
{
    public class CharacterMovedEventArgs : EventArgs
    {
        public string Name { get; private set; }
        public Guid ID { get; private set; }
        public Position From { get; private set; }
        public Position To { get; private set; }

        public CharacterMovedEventArgs(string Name, Guid ID, Position from, Position to)
        {
            this.Name = Name;
            this.ID = ID;
            this.From = from;
            this.To = to;
        }
    }
}
