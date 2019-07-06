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
        public List<Effect> TypicalEffects = new List<Effect>();

        private ResolveByTargetEffect()
        {
            TypeOfEffect = EffectType.ResolveByTarget;
        }

        public ResolveByTargetEffect(List<Effect> TypicalEffects) : this()
        {
            this.TypicalEffects = TypicalEffects;
        }

        public override Outcome Apply(DungeonMaster DM, Character user, Position posTarget, 
            Character charTarget, Deck deck, Card cardDrawn, string optionalFeatures = null)
        {
            var outcome = charTarget.ResolveEffect(DM, user, posTarget, deck, cardDrawn, TypicalEffects);
            return outcome;
        }
    }
}
