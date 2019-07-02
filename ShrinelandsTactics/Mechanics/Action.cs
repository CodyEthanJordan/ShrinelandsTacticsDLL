using System;
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
    public class Action
    {
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public Dictionary<Character.StatType, int> Cost = new Dictionary<Character.StatType, int>();
        [JsonProperty]
        Dictionary<Card, Effect> Effects = new Dictionary<Card, Effect>();
        [JsonProperty]
        Dictionary<CardSource, Card> DeckRecipie = new Dictionary<CardSource, Card>();

        public Action(string Name, Dictionary<Character.StatType, int> Cost, Dictionary<Card,Effect> Effects,
            Dictionary<CardSource, Card> DeckRecipie)
        {
            this.Name = Name;

            this.Cost.Clear();
            foreach (var kvp in Cost)
            {
                this.Cost.Add(kvp.Key, kvp.Value);
            }

            this.Effects.Clear();
            foreach (var kvp in Effects)
            {
                this.Effects.Add(kvp.Key, kvp.Value);
            }

            this.DeckRecipie.Clear();
            foreach (var kvp in DeckRecipie)
            {
                this.DeckRecipie.Add(kvp.Key, kvp.Value);
            }
        }

        public void ResolveAction(DungeonMaster DM, Character user, Position posTarget,
            Character charTarget, string optionalFeatures)
        {
            if(!user.CanPay(this))
            {
                return; //TODO: generate error or raise event?
            }

            //generate outcome deck or mark as uncontested


            //draw card

            //inform user and target what card was drawn, possibly for temporary dodge or breaking shields

            //apply relevant effects
        }

        public enum CardSource
        {
            TargetDodge,
            TargetVitality,
            TargetStamina,
            UserProfeciency,
        }

    }
}
