﻿using System;
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
        public int Value { get; private set; }

        public Condition(string Name, int Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

        //TODO: add handlers for start turn, end turn, so forth?
    }
}