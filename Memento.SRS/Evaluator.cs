using MinimumEditDistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Memento.SRS
{
    public static class Evaluator
    {
        public enum Mark
        {
            Correct,
            Incorrect,
            Typo,
        }

        public static Mark Evaluate(string correctAnswer, string answer, double permissibleError)
        {
            var correctAnswerVariants = correctAnswer.Split('|');

            var correctVariantsWords = from item in correctAnswerVariants select GetWords(item);

            var answerWords = GetWords(answer);

            var marks = from item in correctVariantsWords select Check(item, answerWords, permissibleError);

            if (marks.Any(mark => mark == Mark.Correct))
            {
                return Mark.Correct;
            }
            else if (marks.Any(mark => mark == Mark.Typo))
            {
                return Mark.Typo;
            }
            else
            {
                return Mark.Incorrect;
            }
        }

        private static IEnumerable<string> GetWords(string correctAnswer)
        {
            var words = from word in Regex.Split(correctAnswer, @"\W") select word.ToUpper();

            return words;
        }

        private static Mark Check(IEnumerable<string> correctWords, IEnumerable<string> answerWords, double permissibleError)
        {
            if (correctWords.Count() != answerWords.Count())
            {
                return Mark.Incorrect;
            }
            else
            {
                var zip = Enumerable.Zip(correctWords, answerWords, (x, y) => new { correctAnswer = x, answer = y });

                var marks = from item in zip select Check(item.correctAnswer, item.answer, permissibleError);

                if (marks.Any(mark => mark == Mark.Incorrect))
                {
                    return Mark.Incorrect;
                }
                else if (marks.Any(mark => mark == Mark.Typo))
                {
                    return Mark.Typo;
                }
                else
                {
                    return Mark.Correct;
                }
            }
        }

        private static Mark Check(string correctAnswer, string answer, double permissibleError)
        {
            if (correctAnswer == answer)
            {
                return Mark.Correct;
            }
            else if (correctAnswer.Length <= 1)
            {
                return Mark.Incorrect;
            }
            else
            {
                var distance = Levenshtein.CalculateDistance(correctAnswer, answer, 1);

                var minDistance = correctAnswer.Length * permissibleError;

                var minNonzeroDistance = Math.Max(minDistance, 1);

                if (distance <= minNonzeroDistance)
                {
                    return Mark.Typo;
                }
                else
                {
                    return Mark.Incorrect;
                }
            }
        }
    }
}
