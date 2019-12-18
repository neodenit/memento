using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Neodenit.Memento.Common.DataModels;
using Neodenit.Memento.DataAccess.API;
using Neodenit.Memento.Services;
using Neodenit.Memento.Services.API;

namespace Neodenit.Memento.Tests.Services
{
    [TestClass()]
    public class ExportImportServiceTest
    {
        private ExportImportService sut;

        private Mock<IMementoRepository> mockRepository;
        private Mock<IConverterService> mockConverter;
        private Mock<IValidatorService> mockValidator;
        private Mock<IClozesService> mockClozesService;

        [TestInitialize]
        public void Setup()
        {
            mockRepository = new Mock<IMementoRepository>();
            mockConverter = new Mock<IConverterService>();
            mockValidator = new Mock<IValidatorService>();
            mockClozesService = new Mock<IClozesService>();

            sut = new ExportImportService(mockRepository.Object, mockConverter.Object, mockValidator.Object, mockClozesService.Object);

            mockRepository
                .Setup(x => x.FindDeckAsync(It.IsAny<Guid>()))
                .ReturnsAsync(
                    new Deck
                    {
                        Cards = new List<Card>
                        {
                            new Card
                            {
                                IsValid = true
                            }
                        }
                    });
        }

        [TestMethod()]
        public async Task ExportImportServiceImportTest()
        {
            // Arrange
            var text = "text";
            var id = new Guid("00000000-0000-0000-0000-000000000001");

            // Act
            await sut.Import(text, id);

            // Assert
            mockConverter.Verify(x => x.GetCardsFromDeck(text));
        }

        public async Task ExportImportServiceConvertApkgTest1()
        {
            var filePath = @"Decks\Deck1.apkg";
            var expected = new[] { "Test text with {{c1::cloze}}." };
            await TestApkgConvertion(filePath, expected);
        }

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
            var id = new Guid("00000000-0000-0000-0000-000000000001");

            // Act
            var result = await sut.Export(id);

            // Assert
            mockRepository.Verify(x => x.FindDeckAsync(id));
            mockConverter.Verify(x => x.FormatForExport(It.IsAny<string>(), It.IsAny<string>()));
            Assert.IsNotNull(result);
        }
    }
}