﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}