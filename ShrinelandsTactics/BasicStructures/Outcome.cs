using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrinelandsTactics.BasicStructures
{
    public class Outcome
    {
        public StringBuilder Message { get; private set; }

        public Outcome()
        {
            Message = new StringBuilder();
        }
    }
}
