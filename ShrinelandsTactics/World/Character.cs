using System;
using Newtonsoft.Json;
using ShrinelandsTactics.BasicStructures;

namespace ShrinelandsTactics.World
{
    public class Character
    {
        [JsonProperty]
        public readonly Guid ID;
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public readonly Stat HP;
        [JsonProperty]
        public readonly Stat Move;
        [JsonProperty]
        public readonly Stat Stamina;
        [JsonProperty]
        public readonly Stat Profeciency;
        [JsonProperty]
        public readonly Stat Strength;

        public Character()
        {
            ID = Guid.NewGuid();
            Name = "";
            HP = new Stat();
        }

        public Character(string name, int hp, int move, int stamina, int profeciency, int strength) : this()
        {
            this.Name = name;
            this.HP = new Stat(hp);
            this.Move = new Stat(move);
            this.Stamina = new Stat(stamina);
            this.Profeciency = new Stat(profeciency);
            this.Strength = new Stat(strength);
        }
    }
}