using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Memento.SRS;

namespace Memento.Tests
{
    [TestClass]
    public class ConverterTest
    {
        [TestMethod]
        public void TestReplace()
        {
            var result = Converter.Replace("Test text with {{c1::cloze}}.", "c1", "cloze|alt");

            Assert.AreEqual("Test text with {{c1::cloze|alt}}.", result);
        }

        [TestMethod]
        public void TestReplaceWithHint()
        {
            var result = Converter.Replace("Test text with {{c1::cloze::hint}}.", "c1", "cloze|alt");

            Assert.AreEqual("Test text with {{c1::cloze|alt::hint}}.", result);
        }
    }
}
