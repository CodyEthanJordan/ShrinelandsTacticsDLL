using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ShrinelandsTactics.BasicStructures
{
    [TypeConverter(typeof(PositionTypeConverter))]
    public class Position : IEquatable<Position>
    {
        [JsonProperty]
        public readonly int x;
        [JsonProperty]
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

        public override string ToString()
        {
            return "(" + x + "," + y + ")";
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
            return (x + y) * (x + y + 1) / 2 + x;
        }

        public static Position operator +(Position a, Position b)
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

        public static Position Parse(string posString)
        {
            var parts = posString.Split(' ');
            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);

            return new Position(x, y);
        }
    }

    public class PositionTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                string text = value as string;
                string pattern = @"\((\d+),(\d+)\)";
                Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                Match m = r.Match(text);
                int x = int.Parse(m.Groups[1].Value);
                int y = int.Parse(m.Groups[2].Value);

                //Foo f = JsonConvert.DeserializeObject<Foo>(s);
                return new Position(x,y);
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
