using System;
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

        public int Distance(Position x)
        {
            var diff = this - x;
            return diff.ManhattanMagnitude();
        }

        public int ManhattanMagnitude()
        {
            return Math.Abs(x) + Math.Abs(y);
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
            return new Position(a.x + b.x,
                                a.y + b.y);
        }

        public static Position operator -(Position a, Position b)
        {
            return new Position(a.x - b.x,
                                a.y - b.y);
        }

        public static bool operator ==(Position a, Position b)
        {
            if (((object)a) == null || ((object)b) == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(Position a, Position b)
        {
            if (((object)a) == null || ((object)b) == null)
                return !Object.Equals(a, b);

            return !(a.Equals(b));
        }
    }
}
