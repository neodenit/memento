using Microsoft.VisualStudio.TestTools.UnitTesting;
using Memento.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Memento.Interfaces;
using Moq;

namespace Memento.Tests.Services
{
    [TestClass()]
    public class DecksServiceTest
    {
        private DecksService sut;

        private Mock<IMementoRepository> mockRepository;
        private Mock<IConverter> mockConverter;
        private Mock<IValidator> mockValidator;
        private Mock<IScheduler> mockScheduler;
        private Mock<IDecksService> mockDecksService;

        [TestInitialize]
        public void Setup()
        {
            mockRepository = new Mock<IMementoRepository>();
            mockConverter = new Mock<IConverter>();
            mockValidator = new Mock<IValidator>();
            mockScheduler = new Mock<IScheduler>();
            mockDecksService = new Mock<IDecksService>();

            sut = new DecksService(mockRepository.Object, mockConverter.Object, mockValidator.Object, mockScheduler.Object);
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
        public async Task DecksServiceGetAnswersTest()
        {
            // Arrange
            var id = 1;
            var startTime = DateTime.UtcNow;

            // Act
            var result = await sut.GetAnswersAsync(id, startTime);

            // Assert
            mockRepository.Verify(x => x.GetAnswersForDeckAsync(id), Times.Once);
            Assert.IsNotNull(result);
        }
    }
}