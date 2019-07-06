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
    public class ResolveByTargetEffect : Effect
    {
        public ResolveByTargetEffect()
        {
            TypeOfEffect = EffectType.ResolveByTarget;
        }

        public override void Apply(DungeonMaster DM, Character user, Position posTarget, 
            Character charTarget, Deck deck, Card cardDrawn, string optionalFeatures = null)
        {
            charTarget.ResolveEffect(deck, cardDrawn);
        }
    }
}
