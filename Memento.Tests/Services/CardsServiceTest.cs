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

namespace Memento.Tests.Services
{
    [TestClass()]
    public class CardsServiceTest
    {
        private CardsService sut;

        private Mock<IMementoRepository> mockRepository;
        private Mock<IConverter> mockConverter;
        private Mock<IEvaluator> mockEvaluator;
        private Mock<IFactory> mockFactory;

        [TestInitialize]
        public void Setup()
        {
            mockRepository = new Mock<IMementoRepository>();
            mockConverter = new Mock<IConverter>();
            mockEvaluator = new Mock<IEvaluator>();
            mockFactory = new Mock<IFactory>();

            sut = new CardsService(mockRepository.Object, mockConverter.Object, mockEvaluator.Object, mockFactory.Object);

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
                            },
                        }
                    });
        }

        [TestMethod()]
        public async Task StatServiceGetAnswersTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.GetNextCardAsync(id);

            // Assert
            mockRepository.Verify(x => x.FindDeckAsync(id), Times.Once);
            Assert.IsNotNull(result);
        }
    }
}