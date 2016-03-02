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
            var id = 1;
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
            var answers = Enumerable.Empty<IAnswer>();

            // Act
            var result = sut.GetStatistics(answers);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}