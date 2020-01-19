using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Neodenit.Memento.Common.DataModels;
using Neodenit.Memento.DataAccess.API;
using Neodenit.Memento.Services;

namespace Neodenit.Memento.Tests.Services
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
        public async Task StatServiceGetStatisticsTest()
        {
            // Arrange
            var id = new Guid("00000000-0000-0000-0000-000000000001");
            var startTime = DateTime.UtcNow;

            // Act
            var result = await sut.GetStatisticsAsync(id, startTime);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}