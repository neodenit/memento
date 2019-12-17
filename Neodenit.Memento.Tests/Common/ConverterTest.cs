using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neodenit.Memento.Services.API;
using Neodenit.Memento.Services.Converter;

namespace Neodenit.Memento.Tests
{
    [TestClass]
    public class ConverterTest
    {
        private IConverterService sut;

        [TestInitialize]
        public void Setup()
        {
            var cardOperationService = new CardOperationService(); // TODO: create separate tests
            var rawCardOperationService = new RawCardOperationService(); // TODO: create separate tests
            sut = new ConverterService(cardOperationService, rawCardOperationService);
        }

        [TestMethod]
        public void ReplaceAnswerTest()
        {
            // Arrange

            // Act
            var result = sut.ReplaceAnswer("Test text with {{c1::cloze}}.", "c1", "cloze|alt");

            // Assert
            Assert.AreEqual("Test text with {{c1::cloze|alt}}.", result);
        }

        [TestMethod]
        public void ReplaceAnswerWithHintTest()
        {
            // Arrange

            // Act
            var result = sut.ReplaceAnswer("Test text with {{c1::cloze::hint}}.", "c1", "cloze|alt");

            // Assert
            Assert.AreEqual("Test text with {{c1::cloze|alt::hint}}.", result);
        }
    }
}
