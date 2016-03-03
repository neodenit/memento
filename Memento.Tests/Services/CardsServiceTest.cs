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

        [TestInitialize]
        public void Setup()
        {
            mockRepository = new Mock<IMementoRepository>();

            sut = new CardsService(mockRepository.Object, mockConverter.Object);

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