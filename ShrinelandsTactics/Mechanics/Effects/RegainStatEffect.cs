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
    public class RegainStatEffect : Effect
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty]
        public Character.StatType StatAffected;
        [JsonProperty]
        public int Amount;

        private RegainStatEffect()
        {
            TypeOfEffect = EffectType.RegainStat;
        }

        public RegainStatEffect(Character.StatType stat, int amount) : this()
        {
            this.StatAffected = stat;
            this.Amount = amount;
        }

        public override Outcome Apply(DungeonMaster DM, Character user, Position posTarget, 
            Character charTarget, Deck deck, Card cardDrawn, string optionalFeatures = null)
        {
            var outcome = new Outcome();
            var affected = AffectCaster ? user: charTarget;
            affected.Stats[StatAffected].Regain(Amount);
            outcome.Message.Append(affected.Name + " regins " + Amount + " "
                + StatAffected.ToString());
            return outcome;
        }
    }
}
