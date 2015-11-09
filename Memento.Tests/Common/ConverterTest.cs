using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Memento.Core;

namespace Memento.Tests
{
    [TestClass]
    public class ConverterTest
    {
        private readonly IConverter converter;

        public ConverterTest(IConverter converter)
        {
            this.converter = converter;
        }

        [TestMethod]
        public void TestReplace()
        {
            var result = converter.ReplaceAnswer("Test text with {{c1::cloze}}.", "c1", "cloze|alt");

            Assert.AreEqual("Test text with {{c1::cloze|alt}}.", result);
        }

        [TestMethod]
        public void TestReplaceWithHint()
        {
            var result = converter.ReplaceAnswer("Test text with {{c1::cloze::hint}}.", "c1", "cloze|alt");

            Assert.AreEqual("Test text with {{c1::cloze|alt::hint}}.", result);
        }

        [TestMethod]
        public void TestReplaceWithWildcard2()
        {
            var result = converter.ReplaceTextWithWildcards("Test text with {{c1::cloze}} and another {{c1::cloze}}.", "c1");

            Assert.AreEqual("Test text with {{c1::cloze}} and another {{c1::*}}.", result);
        }

        [TestMethod]
        public void TestReplaceWithWildcard3()
        {
            var result = converter.ReplaceTextWithWildcards("Test text with {{c1::cloze}}, another {{c1::cloze}} and another {{c1::cloze}}.", "c1");

            Assert.AreEqual("Test text with {{c1::cloze}}, another {{c1::*}} and another {{c1::*}}.", result);
        }
    }
}
