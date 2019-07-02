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
    public class NullEffect : Effect
    {
        public NullEffect()
        {
            TypeOfEffect = EffectType.Null;
        }

        public override void Apply(DungeonMaster DM, Character user, Position posTarget, 
            Character charTarget, string optionalFeatures)
        {
        }
    }
}
