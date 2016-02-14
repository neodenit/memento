using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Memento.DomainModel.Models;
using Memento.Interfaces;
using Memento.Common;

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
        public void TestAddNewClozeToEmptyDeck()
        {
            var delay = 8;
            var deck = new Deck { StartDelay = delay };
            var clozes = Enumerable.Empty<Cloze>().ToList();
            var cloze = new Cloze();

            scheduler.PrepareForAdding(deck, clozes, cloze);

            Assert.IsTrue(cloze.IsNew);
            Assert.AreEqual(0, cloze.Position);
            Assert.AreEqual(delay, cloze.LastDelay);
            Assert.IsTrue(Helpers.CheckPositions(clozes));
        }

        [TestMethod]
        public void TestAddNewClozeToNonEmptyDeck()
        {
            var delay = 8;
            var n = 10;
            var deck = new Deck { StartDelay = delay };
            var clozes = (from i in Enumerable.Range(0, n) select new Cloze { Position = i }).ToList();
            var cloze = new Cloze();

            scheduler.PrepareForAdding(deck, clozes, cloze);

            Assert.IsTrue(cloze.IsNew);
            Assert.AreEqual(n, cloze.Position);
            Assert.AreEqual(delay, cloze.LastDelay);
            Assert.IsTrue(Helpers.CheckPositions(clozes));
        }

        [TestMethod]
        public void TestMoveClozeForvard()
        {
            var oldPosition = 10;
            var newPosition = 20;
            var n = 30;
            var clozes = (from i in Enumerable.Range(0, n) select new Cloze { Position = i, ID = i }).ToList();
            var cloze = clozes.Single(item => item.Position == oldPosition);
            var clozeID = cloze.ID;

            scheduler.MoveCloze(clozes, oldPosition, newPosition, cloze.LastDelay, true, true);

            var clozeOnNewPosition = clozes.Single(item => item.Position == newPosition);
            Assert.AreEqual(clozeID, clozeOnNewPosition.ID);
            Assert.AreEqual(newPosition, cloze.Position);
            Assert.IsTrue(Helpers.CheckPositions(clozes));
        }

        [TestMethod]
        public void TestMoveClozeBackward()
        {
            var oldPosition = 20;
            var newPosition = 10;
            var n = 30;
            var clozes = (from i in Enumerable.Range(0, n) select new Cloze { Position = i, ID = i }).ToList();
            var cloze = clozes.Single(item => item.Position == oldPosition);
            var clozeID = cloze.ID;

            scheduler.MoveCloze(clozes, oldPosition, newPosition, cloze.LastDelay, true, true);

            var clozeOnNewPosition = clozes.Single(item => item.Position == newPosition);
            Assert.AreEqual(clozeID, clozeOnNewPosition.ID);
            Assert.AreEqual(newPosition, cloze.Position);
            Assert.IsTrue(Helpers.CheckPositions(clozes));
        }
    }
}
