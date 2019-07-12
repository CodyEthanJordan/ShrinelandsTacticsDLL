using ShrinelandsTactics;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrineMind
{
    public class AIPlayer
    {
        public Guid PlayingAs { get; private set; }

        public AIPlayer(Guid PlayingAs)
        {
            this.PlayingAs = PlayingAs;
        }

        public List<Outcome> TakeTurn(DungeonMaster DM)
        {
            var outcomes = new List<Outcome>();
            if(DM.currentSide.ID != PlayingAs)
            {
                throw new ArgumentException("Its not my turn, I am" + PlayingAs);
            }

            var randomGuy = DM.Characters.First(c => c.SideID == PlayingAs && !c.HasBeenActivated);
            var outcome = DM.Activate(randomGuy);
            outcomes.Add(outcome);

            outcome = DM.MoveCharacter(randomGuy, Map.Direction.S);
            outcomes.Add(outcome);
            outcome = DM.MoveCharacter(randomGuy, Map.Direction.S);
            outcomes.Add(outcome);
            outcome = DM.MoveCharacter(randomGuy, Map.Direction.S);
            outcomes.Add(outcome);
            outcome = DM.MoveCharacter(randomGuy, Map.Direction.S);
            outcomes.Add(outcome);

            outcome = DM.EndTurn(PlayingAs);
            outcomes.Add(outcome);

            return outcomes;
        }
    }
}
