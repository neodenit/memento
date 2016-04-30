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
using System.IO;

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

            sut = new ExportImportService(mockRepository.Object, mockConverter.Object, mockValidator.Object, mockFactory.Object);

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
        public Task ExportImportServiceConvertApkgTest1()
        {
            var filePath = @"Decks\Deck1.apkg";
            var expected = new[] { "Test text with {{c1::cloze}}." };
            return TestApkgConvertion(filePath, expected);
        }

        [TestMethod()]
        public async Task ExportImportServiceConvertApkgTest2()
        {
            var filePath = @"Decks\Deck2.apkg";
            var expected = new[] { "First card with {{c1::cloze}}.", "Second card with {{c1::cloze}}." };
            await TestApkgConvertion(filePath, expected);
        }

        private async Task TestApkgConvertion(string filePath, IEnumerable<string> expected)
        {
            // Arrange
            var file = File.OpenRead(filePath);

            // Act
            var cards = await sut.ConvertApkg(file);

            // Assert
            var orderedCards = cards.OrderBy(x => x);
            var orderedExpectedCards = expected.OrderBy(x => x);

            Assert.IsTrue(orderedExpectedCards.SequenceEqual(orderedCards));
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