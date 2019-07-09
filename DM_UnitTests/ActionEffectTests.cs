using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ShrinelandsTactics;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.World;
using ShrinelandsTactics.Mechanics;
using System.Collections.Generic;
using ShrinelandsTactics.Mechanics.Effects;
using Action = ShrinelandsTactics.Mechanics.Action;

namespace DM_UnitTests
{
    [TestClass]
    public class ActionEffectTests
    {
        private static GameData data = null;

        [ClassInitialize]
        public static void ReadGameData(TestContext context)
        {
            data = GameData.ReadDatafilesInDirectory("GameData");
        }

        [TestMethod]
        public void EffectSerializationTest()
        {
            int damageAmount = 5;
            DamageEffect.DamageType typeOfDamage = DamageEffect.DamageType.Bludgeoning;
            Effect damage1 = new DamageEffect(typeOfDamage, damageAmount);
            string json = JsonConvert.SerializeObject(damage1);
            Effect returnedEffect = JsonConvert.DeserializeObject<Effect>(json);
            DamageEffect damage2 = returnedEffect as DamageEffect;
            Assert.AreEqual(damageAmount, damage2.StaticAmount);
            Assert.AreEqual(typeOfDamage, damage2.TypeOfDamage);
            Assert.AreEqual(Effect.EffectType.Damage, damage2.TypeOfEffect);
        }

        [TestMethod]
        public void ActionSerializationTest()
        {
            var action = DebugData.GetDebugAttackAction();
            string json = JsonConvert.SerializeObject(action);
            var action2 = JsonConvert.DeserializeObject<Action>(json);
            Assert.AreEqual(action.Name, action2.Name);
            Assert.AreEqual(action.Cost.Count, action2.Cost.Count);
            Assert.AreEqual(action.DeckRecipe.Count, action2.DeckRecipe.Count);
        }

        [TestMethod]
        public void CanPayTest()
        {
            var guy = DebugData.GetDebugCharacter();
            var cost = new Dictionary<Character.StatType, int>() { { Character.StatType.Stamina,1} };
            var effects = new Dictionary<Card, Effect>();
            var Recipe = new Dictionary<Action.CardSource, Card>();
            var cheapAction = new Action("test", cost, Recipe, null, ShrinelandsTactics.Mechanics.Action.RangeType.Melee,
                1);
            var cost2 = new Dictionary<Character.StatType, int>() { { Character.StatType.Stamina,999} };
            var expensiveAction = new Action("test", cost2, Recipe, null, ShrinelandsTactics.Mechanics.Action.RangeType.Melee,
                1);

            Assert.IsTrue(guy.CanPay(cheapAction));
            Assert.IsFalse(guy.CanPay(expensiveAction));
        }

        [TestMethod]
        public void TestDrainVitality()
        {
            var DM = DungeonMaster.GetDebugDM(data);
            var drain = data.Actions.FirstOrDefault(a => a.Name == "Drain Vitality");
            var robby = DM.Characters[0];
            robby.Mana.Value = 0;
            var zach = DM.Characters[1];
            zach.Mana.Value = 0;

            var deck = drain.GetDeckFor(DM, robby, null, zach);
            var hitIndex = deck.Cards.IndexOf(deck.Cards.Find(c => c.TypeOfCard == Card.CardType.Hit));
            Deck.FatedDraws.Add("Drain");

            drain.ResolveAction(DM, robby, null, zach, "");

            Assert.IsTrue(robby.Mana.Value > 0);
            Assert.IsTrue(zach.Vitality.Value < zach.Vitality.Max);
            Assert.IsTrue(robby.Vitality.Value == robby.Vitality.Max);
            Assert.IsTrue(zach.Mana.Value == 0);
        }

        [TestMethod]
        public void DebugAttackDeckTest()
        {
            var DM = DungeonMaster.GetDebugDM(data);
            var attack = DebugData.GetDebugAttackAction();

            var robby = DM.Characters[0];
            var zach = DM.Characters[1];

         
            var deck = attack.GetDeckFor(DM, robby, null, zach);

            Assert.AreEqual(robby.Profeciency.Value + robby.WeaponAdvantage - zach.ArmorCoverage, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Hit));
            Assert.AreEqual(zach.ArmorCoverage, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Armor));
            Assert.AreEqual(2, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Miss));

            int dodgeCards = 3;
            zach.Conditions.Add(new Condition("Dodging", dodgeCards));

            deck = attack.GetDeckFor(DM, robby, null, zach);

            Assert.AreEqual(robby.Profeciency.Value + robby.WeaponAdvantage - zach.ArmorCoverage, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Hit));
            Assert.AreEqual(zach.ArmorCoverage, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Armor));
            Assert.AreEqual(2 + dodgeCards, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Miss));
        }

        [TestMethod]
        public void DebugAttackHitTest()
        {
            var DM = DungeonMaster.GetDebugDM(data);
            var attack = DebugData.GetDebugAttackAction();

            var robby = DM.Characters[0];
            var zach = DM.Characters[1];
            zach.Conditions.Add(new Condition("Dodging", 3));

            DM.Activate(robby);
            attack.ResolveAction(DM, robby, null, zach, "");
            //not adjacent so nothing happens
            Assert.IsTrue(zach.Vitality.Value == zach.Vitality.Max);

            zach.Pos = robby.Pos + Map.DirectionToPosition[Map.Direction.E];
            Assert.IsTrue(attack.GetValidTargets(DM, robby).Contains(zach.Pos));
            Deck.SetFate(new List<string>() { "Hit", "Miss" });
            attack.ResolveAction(DM, robby, null, zach, "");
            Assert.IsTrue(zach.Vitality.Value < zach.Vitality.Max);
            Assert.AreEqual(zach.Vitality.Max - robby.Strength.Value - robby.WeaponDamage, zach.Vitality.Value);
        }

        [TestMethod]
        public void DebugAttackCritTest()
        {
            var DM = DungeonMaster.GetDebugDM(data);
            var attack = DebugData.GetDebugAttackAction();

            var robby = DM.Characters[0];
            var zach = DM.Characters[1];
            zach.Conditions.Add(new Condition("Dodging", 3));

            DM.Activate(robby);
            attack.ResolveAction(DM, robby, null, zach, "");
            //not adjacent so nothing happens
            Assert.IsTrue(zach.Vitality.Value == zach.Vitality.Max);

            zach.Pos = robby.Pos + Map.DirectionToPosition[Map.Direction.E];
            Deck.SetFate(new List<string>() { "Hit", "Hit" });
            attack.ResolveAction(DM, robby, null, zach, "");
            Assert.IsTrue(zach.Vitality.Value < zach.Vitality.Max);
            Assert.AreEqual(zach.Vitality.Max - (2*robby.Strength.Value) - robby.WeaponDamage, zach.Vitality.Value);
        }

        [TestMethod]
        public void DebugAttackArmorTest()
        {
            var DM = DungeonMaster.GetDebugDM(data);
            var attack = DebugData.GetDebugAttackAction();

            var robby = DM.Characters[0];
            var zach = DM.Characters[1];
            zach.Conditions.Add(new Condition("Dodging", 3));

            DM.Activate(robby);
            attack.ResolveAction(DM, robby, null, zach, "");
            //not adjacent so nothing happens
            Assert.IsTrue(zach.Vitality.Value == zach.Vitality.Max);

            zach.Pos = robby.Pos + Map.DirectionToPosition[Map.Direction.E];
            Deck.SetFate(new List<string>() { "Glancing Blow" });
            attack.ResolveAction(DM, robby, null, zach, "");
            Assert.IsTrue(zach.Vitality.Value < zach.Vitality.Max);
            Assert.AreEqual(zach.Vitality.Max - robby.Strength.Value - robby.WeaponDamage + zach.ArmorProtection, zach.Vitality.Value);
        }

        [TestMethod]
        public void DebugAttackDodgeTest()
        {
            var DM = DungeonMaster.GetDebugDM(data);
            var attack = DebugData.GetDebugAttackAction();

            var robby = DM.Characters[0];
            var zach = DM.Characters[1];
            int dodgeCards = 3;
            zach.Conditions.Add(new Condition("Dodging", dodgeCards));
            DM.Activate(robby);
            zach.Pos = robby.Pos + Map.DirectionToPosition[Map.Direction.E];

            Deck.SetFate(new List<string>() { "Dodge" });
            attack.ResolveAction(DM, robby, null, zach, "");
            Assert.IsTrue(zach.Vitality.Value == zach.Vitality.Max);
            Assert.AreEqual(dodgeCards - 1, zach.Conditions.First(c => c.Name == "Dodging").Value);
        }

        [TestMethod]
        public void DeserializeAttackTest()
        {
            var DM = DungeonMaster.GetDebugDM(data);
            var robby = DM.Characters[0];

            Assert.IsTrue(robby.Actions.Any(a => a.Name == "Attack"));

            //TODO: assert stuff about attack actions
        }
    }
}
