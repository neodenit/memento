using Memento.Interfaces;
using Memento.Models.Models;
using Memento.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            mockRepository.Setup(x => x.FindDeckAsync(It.IsAny<int>()))
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

            // Act
            await sut.PromoteCloze(deck, delay);

            // Assert
            mockRepository.Verify(x => x.PromoteCloze(deck, delay), Times.Once);
            mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task SchedulerServiceShuffleNewClozes()
        {
            // Arrange
            var id = 1;

            // Act
            await sut.ShuffleNewClozes(id);

            // Assert
            mockRepository.Verify(x => x.FindDeckAsync(id), Times.Once);
            mockScheduler.Verify(x => x.ShuffleNewClozes(It.IsAny<IEnumerable<ICloze>>()), Times.Once);
            mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}
