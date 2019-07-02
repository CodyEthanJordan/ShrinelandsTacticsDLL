﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShrinelandsTactics.Mechanics.Effects
{
    public class Effect
    {
        [JsonProperty]
        public EffectType Type { get; private set; }

        public enum EffectType
        {
            Move,
            Damage,
        }
    }
}