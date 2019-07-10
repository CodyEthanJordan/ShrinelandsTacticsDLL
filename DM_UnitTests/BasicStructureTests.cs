using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ShrinelandsTactics;
using ShrinelandsTactics.BasicStructures;
using ShrinelandsTactics.Mechanics.Effects;
using ShrinelandsTactics.World;

namespace DM_UnitTests
{
    [TestClass]
    public class BasicStructureTests
    {
        
        [TestMethod]
        public void TestPositionEquality()
        {
            var pos1 = new Position(0, 0);
            var pos2 = new Position(1, 1);
            var pos3 = new Position(1, 1);
            Assert.AreNotEqual(pos1, pos2);
            Assert.AreEqual(pos2, pos3);
            Assert.AreEqual(pos2.GetHashCode(), pos3.GetHashCode());
            Assert.IsTrue(pos2 == pos3);
            Assert.AreEqual(pos2, pos2);
        }

        [TestMethod]
        public void TestCombiningCards()
        {
            Deck deck = new Deck();
            Card hit = new Card("Hit", Card.CardType.Hit);
            Card armor = Card.CreateReplacementCard("Glacing Blow", Card.CardType.Armor, hit);

            int numHits = 5;
            int armorCoverage = 3;

            deck.AddCards(hit, numHits);
            deck.AddCards(armor, armorCoverage);
            deck.Consolidate();

            Assert.AreEqual(numHits, deck.Cards.Count);
            Assert.AreEqual(armorCoverage, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Armor));
            Assert.AreEqual(numHits - armorCoverage, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Hit));

            numHits = 3;
            armorCoverage = 10;
            deck = new Deck();

            deck.AddCards(hit, numHits);
            deck.AddCards(armor, armorCoverage);
            deck.Consolidate();

            Assert.AreEqual(numHits, deck.Cards.Count);
            Assert.AreEqual(numHits, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Armor));
            Assert.AreEqual(0, deck.Cards.Count(c => c.TypeOfCard == Card.CardType.Hit));
        }

        [TestMethod]
        public void TestPositionSerialization()
        {
            var pos = new Position(6, 9);
            string json = JsonConvert.SerializeObject(pos);
            var pos2 = JsonConvert.DeserializeObject<Position>(json);
            Assert.AreEqual(pos, pos2);
        }

    }
}
