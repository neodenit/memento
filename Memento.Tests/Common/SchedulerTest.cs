using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Memento.Interfaces;
using Memento.Common;
using Memento.Models.Models;
using Memento.Core.Scheduler;

namespace Memento.Tests
{
    [TestClass]
    public class SchedulerTest
    {
        private IScheduler sut;

        [TestInitialize]
        public void Setup()
        {
            sut = new Scheduler();
        }

        [TestMethod]
        public void AddNewClozeToEmptyDeckTest()
        {
            // Arrange

            var delay = 8;
            var deck = new Deck { StartDelay = delay };
            var clozes = Enumerable.Empty<UserRepetition>().ToList();
            var cloze = new UserRepetition();

            // Act
            sut.PrepareForAdding(deck, clozes, cloze);

            // Assert
            Assert.IsTrue(cloze.IsNew);
            Assert.AreEqual(0, cloze.Position);
            Assert.AreEqual(delay, cloze.LastDelay);
            Assert.IsTrue(Helpers.ArePositionsValid(clozes));
        }

        [TestMethod]
        public void TestAddNewClozeToNonEmptyDeck()
        {
            // Arrange

            var delay = 8;
            var n = 10;
            var deck = new Deck { StartDelay = delay };
            var clozes = (from i in Enumerable.Range(0, n) select new UserRepetition { Position = i }).ToList();
            var cloze = new UserRepetition();

            // Act
            sut.PrepareForAdding(deck, clozes, cloze);

            // Assert
            Assert.IsTrue(cloze.IsNew);
            Assert.AreEqual(n, cloze.Position);
            Assert.AreEqual(delay, cloze.LastDelay);
            Assert.IsTrue(Helpers.ArePositionsValid(clozes));
        }

        [TestMethod]
        public void TestMoveClozeForvard()
        {
            // Arrange

            var oldPosition = 10;
            var newPosition = 20;
            var n = 30;
            var clozes = (from i in Enumerable.Range(0, n)
                          select
                              new UserRepetition
                              {
                                  Position = i,
                                  ID = i,
                                  Cloze = new Cloze { Card = new Card { Deck = new Deck() }  }
                              }).ToList();

            var cloze = clozes.Single(item => item.Position == oldPosition);
            var clozeID = cloze.ID;

            // Act
            sut.MoveRepetition(clozes, oldPosition, newPosition, cloze.LastDelay, true, true);

            // Assert
            var clozeOnNewPosition = clozes.Single(item => item.Position == newPosition);
            Assert.AreEqual(clozeID, clozeOnNewPosition.ID);
            Assert.AreEqual(newPosition, cloze.Position);
            Assert.IsTrue(Helpers.ArePositionsValid(clozes));
        }
    }
}
