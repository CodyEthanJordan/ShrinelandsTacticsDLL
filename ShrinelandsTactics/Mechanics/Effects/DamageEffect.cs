using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShrinelandsTactics.Mechanics.Effects
{
    public class DamageEffect : Effect
    {
        [JsonProperty]
        public DamageType TypeOfDamage { get; private set; }
        [JsonProperty]
        public int Amount { get; set; }

        public DamageEffect()
        {
            TypeOfEffect = EffectType.Damage;
        }

        public DamageEffect(DamageType TypeOfDamage, int Amount) : this()
        {
            this.TypeOfDamage = TypeOfDamage;
            this.Amount = Amount;
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
