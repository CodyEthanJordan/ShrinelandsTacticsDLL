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
    public class ModifyConditionEffect : Effect
    {
        public string Condition;
        public int Amount;

        private ModifyConditionEffect()
        {
            TypeOfEffect = EffectType.ModifyCondition;
        }

        public ModifyConditionEffect(string Condition, int Amount) : this()
        {
            this.Condition = Condition;
            this.Amount = Amount;
        }

        public override Outcome Apply(DungeonMaster DM, Character user, Position posTarget, 
            Character charTarget, Deck deck, Card cardDrawn, string optionalFeatures = null)
        {
            var outcome = new Outcome();
            var affected = AffectCaster ? user: charTarget;
            affected.AddCondition(Condition, Amount);
            outcome.Message.AppendLine(affected.Name + " affected by " + Condition + ":" + Amount);
            return outcome;
        }
    }
}
