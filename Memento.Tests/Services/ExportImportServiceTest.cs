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
    public class ExportImportServiceTest
    {
        private ExportImportService sut;

        private Mock<IMementoRepository> mockRepository;
        private Mock<IConverter> mockConverter;
        private Mock<IValidator> mockValidator;
        private Mock<IFactory> mockFactory;

        [TestInitialize]
        public void Setup()
        {
            mockRepository = new Mock<IMementoRepository>();
            mockConverter = new Mock<IConverter>();
            mockValidator = new Mock<IValidator>();
            mockFactory = new Mock<IFactory>();

            sut = new ExportImportService(mockRepository.Object, mockConverter.Object, mockValidator.Object , mockFactory.Object);

            mockRepository.Setup(x => x.FindDeckAsync(It.IsAny<int>())).ReturnsAsync(new Deck { Cards = new List<Card> { new Card() } });
        }

        [TestMethod()]
        public async Task ExportImportServiceImportTest()
        {
            // Arrange
            var text = "text";
            var id = 1;

            // Act
            await sut.Import(text, id);

            // Assert
            mockConverter.Verify(x => x.GetCardsFromDeck(text));
        }

        [TestMethod()]
        public async Task ExportImportServiceExportTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Export(id);

            // Assert
            mockRepository.Verify(x => x.FindDeckAsync(id));
            mockConverter.Verify(x => x.FormatForExport(It.IsAny<string>(), It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

    }
}