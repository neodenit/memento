using Memento.Core.Evaluators;
using Memento.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Tests.Evaluators
{
    [TestClass]
    public class WordsEvaluatorTest
    {
        private WordsEvaluator sut;

        [TestInitialize]
        public void Setup()
        {
            sut = new WordsEvaluator();
        }

        [TestMethod]
        public void WordsEvaluatorOneWordMatchTest()
        {
            // Arrange
            var correctAnswer = "word";
            var userAnswer = "word";

            // Act
            var result = sut.Evaluate(correctAnswer, userAnswer);

            // Assert
            Assert.AreEqual(Mark.Correct, result);
        }

        [TestMethod]
        public void WordsEvaluatorOneWordMismatchTest()
        {
            // Arrange
            var correctAnswer = "word";
            var userAnswer = "anotherword";

            // Act
            var result = sut.Evaluate(correctAnswer, userAnswer);

            // Assert
            Assert.AreEqual(Mark.Incorrect, result);
        }

        [TestMethod]
        public void WordsEvaluatorTwoWordsMatchTest()
        {
            // Arrange
            var correctAnswer = "word word";
            var userAnswer = "word word";

            // Act
            var result = sut.Evaluate(correctAnswer, userAnswer);

            // Assert
            Assert.AreEqual(Mark.Correct, result);
        }

        [TestMethod]
        public void WordsEvaluatorFunctionTest()
        {
            // Arrange
            var correctAnswer = "function(1)";
            var userAnswer = "function 1";

            // Act
            var result = sut.Evaluate(correctAnswer, userAnswer);

            // Assert
            Assert.AreEqual(Mark.Correct, result);
        }
    }
}
