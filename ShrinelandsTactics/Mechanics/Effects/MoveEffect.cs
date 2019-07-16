using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.World;

namespace ShrinelandsTactics.Mechanics.Effects
{
    public class MoveEffect : Effect
    {

        public override Outcome Apply(DungeonMaster DM, Character user, Position posTarget, 
            Character charTarget, Deck deck, Card cardDrawn, string optionalFeatures = null)
        {
            var outcome = new Outcome();
            var affected = AffectCaster ? user: charTarget;
            outcome.Message.Append(affected.Name + " teleporting to " + posTarget);
            DM.TeleportTo(affected, posTarget);
            return outcome;
        }
    }
}
