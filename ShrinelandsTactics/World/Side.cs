using ShrinelandsTactics.BasicStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShrinelandsTactics.World
{
    public class Side
    {
        [JsonProperty]
        public Guid ID { get; private set; }
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public int Score = 0;

        private Side()
        {
            ID = Guid.NewGuid();
            Name = "";
        }

        public Side(string name) : this()
        {
            this.Name = name;
        }

        public int GetGamestateHash()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ ID.GetHashCode();
                hash = (hash * 16777619) ^ Name.GetHashCode();
                return hash;
            }
        }
    }
}
