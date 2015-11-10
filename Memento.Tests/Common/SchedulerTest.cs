using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Memento.Core;
using Memento.DomainModel.Models;

namespace Memento.Tests
{
    [TestClass]
    public class SchedulerTest
    {
        private readonly IScheduler scheduler;

        public SchedulerTest(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        [TestMethod]
        public void TestAddNewCardToEmptyDeck()
        {
            var delay = 8;
            var deck = new Deck { StartDelay = delay };
            var cards = Enumerable.Empty<Cloze>().ToList();
            var card = new Cloze();

            scheduler.PrepareForAdding(deck, cards, card);

            Assert.IsTrue(card.IsNew);
            Assert.AreEqual(0, card.Position);
            Assert.AreEqual(delay, card.LastDelay);
            Assert.IsTrue(Helpers.CheckPositions(cards));
        }

        [TestMethod]
        public void TestAddNewCardToNonEmptyDeck()
        {
            var delay = 8;
            var n = 10;
            var deck = new Deck { StartDelay = delay };
            var cards = (from i in Enumerable.Range(0, n) select new Cloze { Position = i }).ToList();
            var card = new Cloze();

            scheduler.PrepareForAdding(deck, cards, card);

            Assert.IsTrue(card.IsNew);
            Assert.AreEqual(n, card.Position);
            Assert.AreEqual(delay, card.LastDelay);
            Assert.IsTrue(Helpers.CheckPositions(cards));
        }

        [TestMethod]
        public void TestMoveCardForvard()
        {
            var oldPosition = 10;
            var newPosition = 20;
            var n = 30;
            var cards = (from i in Enumerable.Range(0, n) select new Cloze { Position = i, ID = i }).ToList();
            var card = cards.Single(item => item.Position == oldPosition);
            var cardID = card.ID;

            scheduler.MoveCard(cards, oldPosition, newPosition, card.LastDelay, true, true);

            var cardOnNewPosition = cards.Single(item => item.Position == newPosition);
            Assert.AreEqual(cardID, cardOnNewPosition.ID);
            Assert.AreEqual(newPosition, card.Position);
            Assert.IsTrue(Helpers.CheckPositions(cards));
        }

        [TestMethod]
        public void TestMoveCardBackward()
        {
            var oldPosition = 20;
            var newPosition = 10;
            var n = 30;
            var cards = (from i in Enumerable.Range(0, n) select new Cloze { Position = i, ID = i }).ToList();
            var card = cards.Single(item => item.Position == oldPosition);
            var cardID = card.ID;

            scheduler.MoveCard(cards, oldPosition, newPosition, card.LastDelay, true, true);

            var cardOnNewPosition = cards.Single(item => item.Position == newPosition);
            Assert.AreEqual(cardID, cardOnNewPosition.ID);
            Assert.AreEqual(newPosition, card.Position);
            Assert.IsTrue(Helpers.CheckPositions(cards));
        }
    }
}
