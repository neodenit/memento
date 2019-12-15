using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Neodenit.Memento.Common;
using Neodenit.Memento.Interfaces;
using Neodenit.Memento.Models.DataModels;
using Neodenit.Memento.Services;

namespace Neodenit.Memento.Tests.Services
{
    [TestClass]
    public class SchedulerServiceTest
    {
        private SchedulerService sut;
        private Mock<IMapper> mockMapper;
        private Mock<IMementoRepository> mockRepository;
        private Mock<ISchedulerOperationService> mockScheduler;

        private Guid deckId = new Guid("00000000-0000-0000-0000-000000000001");

        [TestInitialize]
        public void Setup()
        {
            mockMapper = new Mock<IMapper>();
            mockRepository = new Mock<IMementoRepository>();
            mockScheduler = new Mock<ISchedulerOperationService>();

            sut = new SchedulerService(mockMapper.Object, mockRepository.Object, mockScheduler.Object);

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
            var deck = new Deck { ID = deckId };
            var delay = Delays.Same;
            var username = "Username";

            // Act
            await sut.PromoteClozeAsync(deckId, delay, username);

            // Assert
            mockRepository.Verify(x => x.PromoteCloze(deck, delay, username), Times.Once);
            mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task SchedulerServiceShuffleNewClozes()
        {
            // Arrange
            var username = "Username";

            // Act
            await sut.ShuffleNewClozes(deckId, username);

            // Assert
            mockRepository.Verify(x => x.FindDeckAsync(deckId), Times.Once);
            mockScheduler.Verify(x => x.ShuffleNewRepetitions(It.IsAny<IEnumerable<UserRepetition>>()), Times.Once);
            mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}
