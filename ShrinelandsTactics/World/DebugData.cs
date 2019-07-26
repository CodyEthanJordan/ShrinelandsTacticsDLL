using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.Mechanics.Effects;
using ShrinelandsTactics.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = ShrinelandsTactics.Mechanics.Action;
using YamlDotNet.Serialization;
using ShrinelandsTactics.World.Time;

namespace ShrinelandsTactics.World
{
    public static class DebugData
    {
        public static string ForestClimate = @"
- name: Clear Day
  duration: 3
- name: Mist
  duration: 1";

        public static Encounter GetMistWolfEncounter()
        {
            string yaml = @"
Title: Howling in the Mist
Prompt: |
  The wind dies, the sounds of the forest cease, and for a moment there is silence. 

  Howls of pursuing wolves cry out from somewhere in the mist, a terrible and savage sound, yet echoing with a force that no natural throat could produce. The smell of stale and rotting things assaults you as creatures seemingly made of the mist itself pursue, half-seen among the ancient trunks.
Options:
  - Prompt: Flee
    Outcome: SkillCheck
    SkillCheck:
        Cards:
            - Name: Flee
              Number: Condition
            - Name: Pursued
              Number: 4
        Result:
            Flee: 
                Prompt: |
                 The howls fade into the distance and natural sounds resume as you push forward.
                Effect: null
            Pursued:
                Prompt: |
                    The creatures harry you for hours, always at your heels. In time they disappear back into the mist, leaving you with rotting provisions.
                Effect: loose 1 Condition


";

            var deserializer = new Deserializer();
            var encounter = deserializer.Deserialize<Encounter>(yaml);
            return encounter;
        }

        public static List<Weather> GetClimate()
        {
            var deserializer = new Deserializer();
            return deserializer.Deserialize<List<Weather>>(ForestClimate);
        }

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
            var c = new Character("Debug Guy", 10, 5, 4, 3, 3, 4);
            c.Class = "Knight";
            c.Actions.Add(GetDebugAttackAction());
            return c;
        }

        public static Item GetProvisions()
        {
            var item = new Item("Provisions", 5);
            return item;
        }

        public static Action GetDebugAttackAction()
        {
            string Name = "Attack";
            Dictionary<Character.StatType, int> Cost = new Dictionary<Character.StatType, int>()
            { {Character.StatType.Stamina, 1 } };

            Card hit = new Card("Hit", Card.CardType.Hit);

            Dictionary<Action.CardSource, Card> DeckRecipe = new Dictionary<Mechanics.Action.CardSource, Card>()
            {
                {Action.CardSource.UserBaseAttack, hit },
            };

            var hitEffect = new DamageEffect(DamageEffect.DamageType.Slashing, 
                                new List<Action.CardSource>() { Mechanics.Action.CardSource.UserBaseDamage });
            var critEffect = new DamageEffect(DamageEffect.DamageType.Slashing, 
                                new List<Action.CardSource>() { Mechanics.Action.CardSource.UserBaseDamage,
                                                                Mechanics.Action.CardSource.UserStrength});

            //TODO: have numeric resolver?
            var criticalEffects = new Dictionary<Card.CardType, List<Effect>>()
            {
                { Card.CardType.Hit, new List<Effect>() {critEffect} },
                { Card.CardType.Armor, new List<Effect>() {hitEffect} },
                { Card.CardType.Miss, new List<Effect>() {hitEffect} },           
            };


            var effects = new Dictionary<Card.CardType, List<Effect>>()
            {
                {Card.CardType.Miss, new List<Effect>(){new NullEffect()} },
                {Card.CardType.Hit, new List<Effect>(){new RedrawEffect(criticalEffects)} },
                {Card.CardType.Armor, new List<Effect>()
                { new ResolveByTargetEffect(new List<Effect>() { hitEffect })} }
            };

            var action = new Action(Name, Cost, DeckRecipe, effects,
                Mechanics.Action.RangeType.Melee, 1);
            action.Tags.Add(Mechanics.Action.AbilityType.Attack);
            return action;
        }

    }
}
