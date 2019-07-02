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
            Assert.AreEqual(damageAmount, damage2.Amount);
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
            Assert.AreEqual(action.DeckRecipie.Count, action2.DeckRecipie.Count);
        }

        [TestMethod]
        public void CanPayTest()
        {
            var guy = DebugData.GetDebugCharacter();
            var cost = new Dictionary<Character.StatType, int>() { { Character.StatType.Stamina,1} };
            var effects = new Dictionary<Card, Effect>();
            var recipie = new Dictionary<Action.CardSource, Card>();
            var cheapAction = new Action("test", cost, recipie, ShrinelandsTactics.Mechanics.Action.RangeType.Melee,
                1);
            cost[Character.StatType.Stamina] = 9999;
            var expensiveAction = new Action("test", cost, recipie, ShrinelandsTactics.Mechanics.Action.RangeType.Melee,
                1);

            Assert.IsTrue(guy.CanPay(cheapAction));
            Assert.IsFalse(guy.CanPay(expensiveAction));
        }

        [TestMethod]
        public void BasicDeckBuildingTest()
        {
            var DM = DungeonMaster.GetDebugDM(data);
            var guy1 = DebugData.GetDebugCharacter();
            var guy2 = DebugData.GetDebugCharacter();
            var cost = new Dictionary<Character.StatType, int>() { { Character.StatType.Stamina, 1 } };
            var nullEffect = new NullEffect();
            var recipie = new Dictionary<Action.CardSource, Card>()
            {
                { Action.CardSource.TargetDodge, new Card("Dodge", Card.CardType.Miss, nullEffect) },
                { Action.CardSource.UserProfeciency, new Card("Hit", Card.CardType.Hit, nullEffect) },
            };

            var action = new Action("test", cost, recipie, ShrinelandsTactics.Mechanics.Action.RangeType.Melee, 1);
            Deck deck = action.GetDeckFor(DM, guy1, null, guy2);

            Assert.AreEqual(guy1.Profeciency.Value, deck.Cards.Count(c => c.Name == "Hit"));
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

            drain.ResolveAction(DM, robby, null, zach, "", hitIndex);

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

            Assert.AreEqual(robby.Profeciency.Value - 2, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Hit));
            Assert.AreEqual(2, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Armor));
            Assert.AreEqual(2, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Miss));

            zach.Conditions.Add(new Condition("Dodging", 3));

            deck = attack.GetDeckFor(DM, robby, null, zach);

            Assert.AreEqual(robby.Profeciency.Value - 2, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Hit));
            Assert.AreEqual(2, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Armor));
            Assert.AreEqual(5, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Miss));

            var glacingBlow = deck.Cards.FirstOrDefault(c => c.TypeOfCard == Card.CardType.Armor);
            DamageEffect armorDamage = glacingBlow.Effects[0] as DamageEffect;
            var hit = deck.Cards.FirstOrDefault(c => c.TypeOfCard == Card.CardType.Hit);
            DamageEffect hitDamage = hit.Effects[0] as DamageEffect;

            Assert.IsTrue(armorDamage.Amount < hitDamage.Amount);

        }

        [TestMethod]
        public void DebugAttackOutcomeTest()
        {
            var DM = DungeonMaster.GetDebugDM(data);
            var attack = DebugData.GetDebugAttackAction();

            var robby = DM.Characters[0];
            var zach = DM.Characters[1];
            zach.Conditions.Add(new Condition("Dodging", 3));

            var deck = attack.GetDeckFor(DM, robby, null, zach);
            int hitIndex = deck.Cards.IndexOf(deck.Cards.First(c => c.TypeOfCard == Card.CardType.Hit));

            attack.ResolveAction(DM, robby, null, zach, "", hitIndex);
            //not adjacent so nothing happens
            Assert.IsTrue(zach.Vitality.Value == zach.Vitality.Max);

            zach.Pos = robby.Pos + new Position(1, 0);
            attack.ResolveAction(DM, robby, null, zach, "", hitIndex);
            Assert.IsTrue(zach.Vitality.Value < zach.Vitality.Max);
        }
    }
}
