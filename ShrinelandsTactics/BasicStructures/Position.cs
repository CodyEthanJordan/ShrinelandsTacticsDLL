﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrinelandsTactics.BasicStructures
{
    public class Position : IEquatable<Position>
    {
        public readonly int x;
        public readonly int y;

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Position other)
        {
            return this.x == other.x && this.y == other.y;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Position);
        }

        public override int GetHashCode()
        {
            //use Cantor's pairing function to create unique hash
            return (x+y)*(x+y+1)/2 + x;
        }

        public static Position operator + (Position a, Position b)
        {
            return new Position(a.x + b.y,
                                a.y + b.y);
        }
    }
}
