using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShrinelandsTactics.BasicStructures
{
    public class Stat
    {
        [JsonIgnore]
        private int _value;
        [JsonIgnore]
        private int _max;

        [JsonProperty]
        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if(OnStatChanged != null)
                {
                    OnStatChanged(this, new Events.StatChangedEventArgs(Guid.Empty, World.Character.StatType.Mana, this));
                }
            }
        }


        [JsonProperty]
        public int Max
        {
            get { return _max; } 
            set
            {
                _max = value;
                if (OnStatChanged != null)
                {
                    OnStatChanged(this, new Events.StatChangedEventArgs(Guid.Empty, World.Character.StatType.Mana, this));
                }
            }
        }

        public event DungeonMaster.StatChnagedEventHandler OnStatChanged;

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

        public void Regain(int amount)
        {
            Value = Math.Min(Value + amount, Max);
        }

        public override string ToString()
        {
            return Value + "/" + Max;
        }
    }
}
