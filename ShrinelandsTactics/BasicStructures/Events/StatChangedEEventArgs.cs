using ShrinelandsTactics.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrinelandsTactics.BasicStructures.Events
{
    public class StatChangedEventArgs : EventArgs
    {
        public Guid CharacterID;
        public Character.StatType StatAffected;
        public Stat NewStat;

        public StatChangedEventArgs(Guid CharacterID, Character.StatType StatAffected, Stat NewStat)
        {
            this.CharacterID = CharacterID;
            this.StatAffected = StatAffected;
            this.NewStat = NewStat;
        }
    }
}
