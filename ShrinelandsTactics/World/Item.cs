﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrinelandsTactics.World
{
    public class Item
    {
        public string Name;
        public int Count;

        public Item(string name, int count = 1)
        {
            this.Name = name;
            this.Count = count;
        }
    }
}
