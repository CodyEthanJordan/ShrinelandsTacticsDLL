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
        public int Amount { get; set; }

        private DamageEffect()
        {
            TypeOfEffect = EffectType.Damage;
        }

        public DamageEffect(DamageType TypeOfDamage, int Amount) : this()
        {
            this.TypeOfDamage = TypeOfDamage;
            this.Amount = Amount;
        }

        public override void Apply(DungeonMaster DM, Character user, Position posTarget, 
            Character charTarget, string optionalFeatures)
        {
            charTarget.TakeDamage(TypeOfDamage, Amount);
        }

        public enum DamageType
        {
            Slashing,
            Piercing,
            Bludgeoning,
            Fire,
        }
    }
}
