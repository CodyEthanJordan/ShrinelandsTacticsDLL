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

        public Side()
        {
            ID = Guid.NewGuid();
            Name = "";
        }

        public Side(string name) : this()
        {
            this.Name = name;
        }
    }
}
