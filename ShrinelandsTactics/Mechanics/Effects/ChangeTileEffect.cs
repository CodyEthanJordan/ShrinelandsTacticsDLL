using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.World;

namespace ShrinelandsTactics.Mechanics.Effects
{
    public class ChangeTileEffect : Effect
    {
        [JsonProperty]
        public string ReplacingTile { get; private set; }

        private ChangeTileEffect()
        {
            TypeOfEffect = EffectType.Damage;
        }

        public ChangeTileEffect(string tile) : this()
        {
            this.ReplacingTile = tile;
        }

        public override Outcome Apply(DungeonMaster DM, Character user, Position posTarget, 
            Character charTarget, Deck deck, Card cardDrawn, string optionalFeatures = null)
        {
            var outcome = new Outcome();
            if(posTarget == null)
            {
                throw new ArgumentException("Target cannot be null for changing tile");
            }
            var tile = DM.data.GetTileByName(ReplacingTile);
            DM.map.MakeTile(tile, posTarget, DM.data);

            var charStanding = DM.Characters.FirstOrDefault(c => c.Pos == posTarget);
            if(charStanding != null)
            {
                var newTile = DM.map.GetTile(posTarget);
                newTile.CharacterEntered(DM, charStanding);
            }

            return outcome;
        }
    }
}
