using ShrinelandsTactics.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrinelandsTactics.AI
{
    public class BoardStateEvaluator
    {
        public float vitalityWeight = 10;
        public float staminaWeight = 1;
        public float distanceWeight = 0.5f;

        public double EvaluateBoardState(DungeonMaster DM, Guid SideID)
        {
            double score = 0;
            foreach (var guy in DM.Characters) //TODO: parallelize?
            {
                int mySide = guy.SideID == SideID ? 1 : -1;
                score += EvaluateCharacterScore(DM, guy) * mySide;
            }

            return score;
        }

        public double EvaluateCharacterScore(DungeonMaster DM, Character guy)
        {
            double score = 0;

            score += guy.Vitality.Value;
            score += guy.Stamina.Value;

            var enemyDistance = DM.Characters.Where(c => c.ID != guy.ID)
                .Where(c => c.SideID != guy.SideID)
                .Select(c => c.Pos.Distance(guy.Pos))
                .Min();

            score += enemyDistance * distanceWeight;

            return score;

        }

        public List<DungeonMaster> GetAllPossibleStates(DungeonMaster DM, Character guy)
        {
            var states = new List<DungeonMaster>();

            var moves = DM.GetPossibleMoves(guy);

            throw new NotImplementedException();
        }
    }
}
