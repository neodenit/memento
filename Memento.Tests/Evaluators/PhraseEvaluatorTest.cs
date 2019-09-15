using Memento.Core.Evaluators;
using Memento.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Memento.Tests.Evaluators
{
    [TestClass]
    public class PhraseEvaluatorTest
    {
        private PhraseEvaluator sut;

        [TestInitialize]
        public void Setup()
        {
            sut = new PhraseEvaluator();
        }

        [TestMethod]
        public void PhraseEvaluatorOneWordMatchTest()
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
        public void PhraseEvaluatorOneWordMismatchTest()
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
        public void PhraseEvaluatorTwoWordsMatchTest()
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
        public void PhraseEvaluatorFunctionTest()
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
