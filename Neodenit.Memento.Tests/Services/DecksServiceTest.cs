using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Neodenit.Memento.Interfaces;
using Neodenit.Memento.Models.DataModels;
using Neodenit.Memento.Models.ViewModels;
using Neodenit.Memento.Services;

namespace Neodenit.Memento.Tests.Services
{
    [TestClass()]
    public class DecksServiceTest
    {
        private DecksService sut;

        private Mock<IMementoRepository> mockRepository;

        private Guid deckId = new Guid("00000000-0000-0000-0000-000000000001");

        [TestInitialize]
        public void Setup()
        {
            mockRepository = new Mock<IMementoRepository>();

            sut = new DecksService(mockRepository.Object);

            mockRepository.Setup(x => x.FindDeckAsync(It.IsAny<Guid>())).ReturnsAsync(new Deck());
        }

        [TestMethod()]
        public async Task DecksServiceGetDecksTest()
        {
            // Arrange
            var userName = "user@server.com";

            // Act
            var result = await sut.GetDecksAsync(userName);

            // Assert
            mockRepository.Verify(x => x.GetUserDecksAsync(It.IsAny<string>()), Times.Once);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task DecksServiceGetDeckWithStatViewModelTest()
        {
            // Arrange
            var id = deckId;
            var statistics = new StatisticsViewModel();
            var username = "Username";

            // Act
            var result = await sut.GetDeckWithStatViewModel(id, statistics, username);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Deck);
            Assert.IsNotNull(result.Stat);
        }

        [TestMethod()]
        public async Task DecksServiceFindDeckTest()
        {
            // Arrange
            var id = deckId;

            // Act
            var result = await sut.FindDeckAsync(id);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task DecksServiceCreateDeckTest()
        {
            // Arrange
            var deck = new Deck();
            var userName = "user@server.com";

            // Act
            await sut.CreateDeck(deck, userName);

            // Assert
            mockRepository.Verify(x => x.AddDeck(deck), Times.Once);
            mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [TestMethod()]
        public async Task DecksServiceUpdateDeckTest()
        {
            // Arrange
            var id = deckId;
            var title = "Title";
            var startDelay = 8;
            var coeff = 2.0;
            var previewAnswer = false;

            // Act
            await sut.UpdateDeck(id, title, startDelay, coeff, previewAnswer);

            // Assert
            mockRepository.Verify(x => x.FindDeckAsync(id), Times.Once);
            mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [TestMethod()]
        public async Task DecksServiceDeleteDeckTest()
        {
            // Arrange
            var id = deckId;

            // Act
            await sut.DeleteDeck(id);

            // Assert
            mockRepository.Verify(x => x.FindDeckAsync(id), Times.Once);
            mockRepository.Verify(x => x.RemoveDeck(It.IsAny<Deck>()), Times.Once);
            mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}