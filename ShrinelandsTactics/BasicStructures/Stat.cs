﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShrinelandsTactics.BasicStructures
{
    public class Stat
    {
        [JsonProperty]
        public int Value { get; private set; }
        [JsonProperty]
        public int Max { get; private set; }

        public Stat()
        {
            Value = 0;
            Max = 0;
        }

        public Stat(int max) : this(max, max)
        {

        }

        public Stat(int value, int max)
        {
            this.Value = value;
            this.Max = max;
        }
    }
}
