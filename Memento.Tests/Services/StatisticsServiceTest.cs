using System;
using System.Linq;
using System.Threading.Tasks;
using Memento.Interfaces;
using Memento.Models.Models;
using Memento.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Memento.Tests.Services
{
    [TestClass()]
    public class StatisticsServiceTest
    {
        private StatisticsService sut;

        private Mock<IMementoRepository> mockRepository;

        [TestInitialize]
        public void Setup()
        {
            mockRepository = new Mock<IMementoRepository>();

            sut = new StatisticsService(mockRepository.Object);
        }

        [TestMethod()]
        public async Task StatServiceGetAnswersTest()
        {
            // Arrange
            var id = new Guid("00000000-0000-0000-0000-000000000001");
            var startTime = DateTime.UtcNow;

            // Act
            var result = await sut.GetAnswersAsync(id, startTime);

            // Assert
            mockRepository.Verify(x => x.GetAnswersForDeckAsync(id), Times.Once);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void StatServiceGetStatisticsTest()
        {
            // Arrange
            var answers = Enumerable.Empty<Answer>();

            // Act
            var result = sut.GetStatistics(answers);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}