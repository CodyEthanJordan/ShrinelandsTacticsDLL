using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.Mechanics.Effects;
using ShrinelandsTactics.World;

namespace ShrinelandsTactics.Mechanics
{
    public class Action
    {
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public Dictionary<Character.StatType, int> Cost = new Dictionary<Character.StatType, int>();
        [JsonProperty]
        List<Effect> Effects = new List<Effect>();



        public Action(string Name, Dictionary<Character.StatType, int> Cost, List<Effect> Effects)
        {
            this.Name = Name;

            this.Cost.Clear();
            foreach (var kvp in Cost)
            {
                this.Cost.Add(kvp.Key, kvp.Value);
            }

            this.Effects.Clear();
            foreach (var effect in Effects)
            {
                this.Effects.Add(effect);
            }
        }

        public void ResolveAction(DungeonMaster DM, Character user, Position posTarget,
            Character charTarget, string optionalFeatures)
        {
            if(!user.CanPay(this))
            {
                return; //TODO: generate error or raise event?
            }
        }

    }
}
