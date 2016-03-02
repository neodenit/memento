using Microsoft.VisualStudio.TestTools.UnitTesting;
using Memento.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Memento.Interfaces;
using Moq;
using Memento.Models.Models;
using Memento.Additional;

namespace Memento.Tests.Services
{
    [TestClass()]
    public class DecksServiceTest
    {
        private DecksService sut;

        private Mock<IMementoRepository> mockRepository;

        [TestInitialize]
        public void Setup()
        {
            mockRepository = new Mock<IMementoRepository>();

            sut = new DecksService(mockRepository.Object);

            mockRepository.Setup(x => x.FindDeckAsync(It.IsAny<int>())).ReturnsAsync(new Deck());
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
            var id = 1;
            var statistics = new Statistics();

            // Act
            var result = await sut.GetDeckWithStatViewModel(id, statistics);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Deck);
            Assert.IsNotNull(result.Stat);
        }

        [TestMethod()]
        public async Task DecksServiceFindDeckTest()
        {
            // Arrange
            var id = 1;

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
            var id = 1;
            var title = "Title";
            var startDelay = 8;
            var coeff = 2.0;

            // Act
            await sut.UpdateDeck(id, title, startDelay, coeff);

            // Assert
            mockRepository.Verify(x => x.FindDeckAsync(id), Times.Once);
            mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [TestMethod()]
        public async Task DecksServiceDeleteDeckTest()
        {
            // Arrange
            var id = 1;

            // Act
            await sut.DeleteDeck(id);

            // Assert
            mockRepository.Verify(x => x.FindDeckAsync(id), Times.Once);
            mockRepository.Verify(x => x.RemoveDeck(It.IsAny<IDeck>()), Times.Once);
            mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}