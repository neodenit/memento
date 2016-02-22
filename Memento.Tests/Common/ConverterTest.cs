using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Memento.Interfaces;
using Memento.Core;

namespace Memento.Tests
{
    [TestClass]
    public class ConverterTest
    {
        private IConverter sut;

        [TestInitialize]
        public void Setup()
        {
            sut = new Converter();
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

        [TestMethod]
        public void ReplaceTwoClozesWithWildcardsTest()
        {
            // Arrange

            // Act
            var result = sut.ReplaceTextWithWildcards("Test text with {{c1::cloze}} and another {{c1::cloze}}.", "c1");

            // Assert
            Assert.AreEqual("Test text with {{c1::cloze}} and another {{c1::*}}.", result);
        }

        [TestMethod]
        public void ReplaceThreeClozesWithWildcardTest()
        {
            // Arrange

            // Act
            var result = sut.ReplaceTextWithWildcards("Test text with {{c1::cloze}}, another {{c1::cloze}} and another {{c1::cloze}}.", "c1");

            // Assert
            Assert.AreEqual("Test text with {{c1::cloze}}, another {{c1::*}} and another {{c1::*}}.", result);
        }
    }
}
