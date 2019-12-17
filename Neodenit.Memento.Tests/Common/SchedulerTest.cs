using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neodenit.Memento.Common.DataModels;
using Neodenit.Memento.Services;
using Neodenit.Memento.Services.API;
using Neodenit.Memento.Services.Scheduler;

namespace Neodenit.Memento.Tests
{
    [TestClass]
    public class SchedulerTest
    {
        private ISchedulerOperationService sut;

        [TestInitialize]
        public void Setup()
        {
            var cardOperationService = new SchedulerUtilsService(); // TODO: create separate tests

            sut = new SchedulerOperationService(cardOperationService);
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
            Assert.IsTrue(ModelHelpers.ArePositionsValid(clozes));
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
            Assert.IsTrue(ModelHelpers.ArePositionsValid(clozes));
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
            Assert.IsTrue(ModelHelpers.ArePositionsValid(clozes));
        }
    }
}
