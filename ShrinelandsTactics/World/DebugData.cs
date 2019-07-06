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
            var c = new Character("Debug Guy", 10, 5, 4, 3, 3, 0);
            c.Actions.Add(GetDebugAttackAction());
            return c;
        }

        public static Action GetDebugAttackAction()
        {
            string Name = "Attack";
            Dictionary<Character.StatType, int> Cost = new Dictionary<Character.StatType, int>()
            { {Character.StatType.Stamina, 1 } };

            Card hit = new Card("Hit", Card.CardType.Hit);

            Dictionary<Action.CardSource, Card> DeckRecipie = new Dictionary<Mechanics.Action.CardSource, Card>()
            {
                {Action.CardSource.UserBaseAttack, hit },
            };

            //TODO: have numeric resolver?
            var criticalEffects = new Dictionary<Card.CardType, List<Effect>>()
            {
                { Card.CardType.Hit, new List<Effect>() {new DamageEffect(DamageEffect.DamageType.Bludgeoning, 5)} },
                { Card.CardType.Armor, new List<Effect>() {new DamageEffect(DamageEffect.DamageType.Bludgeoning, 2)} },
                { Card.CardType.Miss, new List<Effect>() {new DamageEffect(DamageEffect.DamageType.Bludgeoning, 2)} },           
            };


            var effects = new Dictionary<Card.CardType, List<Effect>>()
            {
                {Card.CardType.Miss, new List<Effect>(){new NullEffect()} },
                {Card.CardType.Hit, new List<Effect>(){new RedrawEffect(criticalEffects)} },
                {Card.CardType.Armor, new List<Effect>(){new ResolveByTargetEffect()} }
            };

            var action = new Action(Name, Cost, DeckRecipie, effects,
                Mechanics.Action.RangeType.Melee, 1);
            action.Tags.Add(Mechanics.Action.AbilityType.Attack);
            return action;
        }

    }
}
