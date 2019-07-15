using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.World;

namespace ShrinelandsTactics.Mechanics.Effects
{
    public class DamageEffect : Effect
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty]
        public DamageType TypeOfDamage { get; private set; }
        [JsonProperty]
        public int StaticAmount { get; set; }
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<Action.CardSource> Sources = null;

        public int GetAmount(DungeonMaster DM, Character user, Position posTarget,
            Character charTarget)
        {
            if (Sources != null)
            {
                int amount = 0;
                foreach (var s in Sources)
                {
                    amount += Action.ResolveSource(s, DM, user, posTarget, charTarget);
                }
                return amount;
            }
            else
            {
                return StaticAmount;
            }
        }

        private DamageEffect()
        {
            TypeOfEffect = EffectType.Damage;
        }

        public DamageEffect(DamageType TypeOfDamage, int Amount) : this()
        {
            this.TypeOfDamage = TypeOfDamage;
            this.StaticAmount = Amount;
        }

        public DamageEffect(DamageType TypeOfDamage, List<Action.CardSource> Sources) : this()
        {
            this.Sources = Sources;
        }


        public override Outcome Apply(DungeonMaster DM, Character user, Position posTarget, 
            Character charTarget, Deck deck, Card cardDrawn, string optionalFeatures = null)
        {
            var outcome = new Outcome();
            var affected = AffectCaster ? user: charTarget;
            int amount = StaticAmount;
            if(Sources != null)
            {
                amount = 0;
                foreach (var s in Sources)
                {
                    amount += Action.ResolveSource(s, DM, user, posTarget, charTarget);
                }
            }
            
            affected.TakeDamage(TypeOfDamage, amount);
            outcome.Message.AppendLine(affected.Name + " takes " + amount + " " + TypeOfDamage);
            return outcome;
        }

        public enum DamageType
        {
            Slashing,
            Piercing,
            Bludgeoning,
            Fire,
            Magic,
            True,
        }
    }
}
