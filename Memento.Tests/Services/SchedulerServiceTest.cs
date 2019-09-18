using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Memento.Interfaces;
using Memento.Models.Models;
using Memento.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Memento.Tests.Services
{
    [TestClass]
    public class SchedulerServiceTest
    {
        private SchedulerService sut;

        private Mock<IMementoRepository> mockRepository;
        private Mock<IScheduler> mockScheduler;

        [TestInitialize]
        public void Setup()
        {
            mockRepository = new Mock<IMementoRepository>();
            mockScheduler = new Mock<IScheduler>();

            sut = new SchedulerService(mockRepository.Object, mockScheduler.Object);

            mockRepository.Setup(x => x.FindDeckAsync(It.IsAny<Guid>()))
                .ReturnsAsync(
                    new Deck
                    {
                        Cards = new List<Card>
                        {
                            new Card
                            {
                                IsValid = true,
                                Clozes = new List<Cloze>
                                {
                                    new Cloze()
                                }
                            }
                        }
                    });
        }

        [TestMethod]
        public async Task SchedulerServicePromoteClozeTest()
        {
            // Arrange
            var deck = new Deck();
            var delay = Delays.Same;
            var username = "Username";

            // Act
            await sut.PromoteCloze(deck, delay, username);

            // Assert
            mockRepository.Verify(x => x.PromoteCloze(deck, delay, username), Times.Once);
            mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task SchedulerServiceShuffleNewClozes()
        {
            // Arrange
            var id = new Guid("00000000-0000-0000-0000-000000000001");
            var username = "Username";

            // Act
            await sut.ShuffleNewClozes(id, username);

            // Assert
            mockRepository.Verify(x => x.FindDeckAsync(id), Times.Once);
            mockScheduler.Verify(x => x.ShuffleNewRepetitions(It.IsAny<IEnumerable<UserRepetition>>()), Times.Once);
            mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}
