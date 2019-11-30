using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Memento.Interfaces;
using Memento.Models.Models;
using Memento.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Memento.Tests.Services
{
    [TestClass()]
    public class CardsServiceTest
    {
        private CardsService sut;

        private Mock<IMementoRepository> mockRepository;
        private Mock<IConverterService> mockConverter;
        private Mock<IEvaluatorService> mockEvaluator;

        private string userName = "user@server.com";

        [TestInitialize]
        public void Setup()
        {
            mockRepository = new Mock<IMementoRepository>();
            mockConverter = new Mock<IConverterService>();
            mockEvaluator = new Mock<IEvaluatorService>();

            sut = new CardsService(mockRepository.Object, mockConverter.Object, mockEvaluator.Object);

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
                                    new Cloze
                                    {
                                        UserRepetitions = new List<UserRepetition>
                                        {
                                            new UserRepetition
                                            {
                                                UserName = userName
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });
        }

        [TestMethod()]
        public async Task StatServiceGetAnswersTest()
        {
            // Arrange
            var id = new Guid("00000000-0000-0000-0000-000000000001");

            // Act
            var result = await sut.GetNextCardAsync(id, userName);

            // Assert
            mockRepository.Verify(x => x.FindDeckAsync(id), Times.Once);
            Assert.IsNotNull(result);
        }
    }
}