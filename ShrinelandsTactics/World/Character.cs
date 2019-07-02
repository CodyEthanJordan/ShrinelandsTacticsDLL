using System;
using Newtonsoft.Json;
using ShrinelandsTactics.BasicStructures;

namespace ShrinelandsTactics.World
{
    public class Character
    {
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public readonly Stat HP;

        public Character()
        {
            Name = "";
            HP = new Stat();
        }

        public Character(string name, int hp)
        {
            this.Name = name;
            this.HP = new Stat(hp);
        }
    }
}