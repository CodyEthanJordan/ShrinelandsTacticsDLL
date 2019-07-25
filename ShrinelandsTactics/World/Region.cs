using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrinelandsTactics.World
{
    public class Region
    {
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public List<Weather> Climate;

        public Region(string Name)
        {
            this.Name = Name;

            SetupDebugData();

        }

        private void SetupDebugData()
        {
            this.Climate = DebugData.GetClimate();
        }

        internal Weather RollWeather()
        {
            int i = DungeonMaster.rand.Next(0, Climate.Count);
            return Climate[i];
        }
    }
}
