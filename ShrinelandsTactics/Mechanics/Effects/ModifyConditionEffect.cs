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
        public ModifyConditionEffect(string condition, int amount, Character target)
        {
            TypeOfEffect = EffectType.ModifyCondition;
        }
        public override void Apply(DungeonMaster DM, Character user, Position posTarget, 
            Character charTarget, string optionalFeatures)
        {
            throw new NotImplementedException();
        }
    }
}
