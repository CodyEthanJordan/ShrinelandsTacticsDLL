using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShrinelandsTactics.BasicStructures
{
    public class Outcome
    {
        [JsonIgnore]
        public StringBuilder Message { get; private set; }
        public string ActionTaken;
        public Guid UserID;
        public Guid TargetID = Guid.Empty;
        public Position PosTarget;
        public List<Card> CardsDrawn = new List<Card>();

        public Outcome()
        {
            Message = new StringBuilder();
        }
    }
}
