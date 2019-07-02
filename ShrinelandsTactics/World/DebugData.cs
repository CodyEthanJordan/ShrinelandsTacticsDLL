using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.Mechanics.Effects;
using ShrinelandsTactics.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = ShrinelandsTactics.Mechanics.Action;

namespace ShrinelandsTactics.World
{
    public static class DebugData
    {
        public static Map GetFlatlandMap(GameData data)
        {
            string description =
@"Flatland
15 20
###############
#.............#
#.............#
#.............#
#.......#.....#
#.............#
#.#...........#
#.............#
#........#....#
#.............#
#.....###.....#
#.............#
#.............#
#.............#
#.............#
#.............#
#.............#
#.............#
#.............#
###############
";

            return Map.CreateFromText(description, data);
        }

        public static Tile GetEmptyTile()
        {
            var properties = new List<Tile.TileProperties>() { Tile.TileProperties.DebugProperty };
            return new Tile("DebugEmpty", true, 1, 'e', properties);
        }

        public static Character GetDebugCharacter()
        {
            return new Character("Debug Guy", 10, 5, 4, 3, 3, 0);
        }

        public static Action GetDebugAttackAction()
        {
            string Name = "DebugAttack";
            Dictionary<Character.StatType, int> Cost = new Dictionary<Character.StatType, int>()
            { {Character.StatType.Stamina, 1 } };
            Effect dealDamage = new DamageEffect(DamageEffect.DamageType.Slashing, 5);
            Effect none = new NullEffect();

            Card hit = new Card("Hit", Card.CardType.Hit, dealDamage);
            Card armor = Card.CreateReplacementCard("Glancing Blow", Card.CardType.Armor, dealDamage, hit);
            Card miss = new Card("Dodge", Card.CardType.Miss, none);

            Dictionary<Action.CardSource, Card> DeckRecipie = new Dictionary<Mechanics.Action.CardSource, Card>()
            {
                {Action.CardSource.UserProfeciency, hit },
                {Action.CardSource.TargetDodge, miss },
                {Action.CardSource.TargetArmorCoverage, armor }
            };

            var action = new Action(Name, Cost, DeckRecipie);
            return action;
        }

    }
}
