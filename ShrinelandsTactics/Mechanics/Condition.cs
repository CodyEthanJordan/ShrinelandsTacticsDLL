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
    public class Condition
    {
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public int Value { get; set; }
        [JsonProperty]
        public int Duration { get; private set; }

        public Condition(string Name, int Value, int Duration=-1)
        {
            this.Name = Name;
            this.Value = Value;
            this.Duration = Duration;
        }

        public void StartingActivation()
        {
            Duration--;
        }

        public override string ToString()
        {
            return Name + ":" + Value;
        }

        //TODO: add handlers for start turn, end turn, so forth?
    }
}
