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
        public static bool IsCorrect(string correctAnswer, string answer)
        public static bool IsCorrect(string correctAnswer, string answer, double permissibleError)
        {
            var correctAnswerVariants = correctAnswer.Split('|');

            var correctVariantsWords = from item in correctAnswerVariants select GetWords(item);

            var answerWords = GetWords(answer);

            var result = correctVariantsWords.Any(item => Compare(item, answerWords, permissibleError));

            return result;
        }

        private static IEnumerable<string> GetWords(string correctAnswer)
        {
            var words = from Match match in Regex.Split(correctAnswer, @"\W") select match.Value.ToUpper();

            return words;
        }

        private static bool Compare(IEnumerable<string> correctWords, IEnumerable<string> answerWords, double permissibleError)
        {
            if (correctWords.Count() != answerWords.Count())
            {
                return false;
            }
            else
            {
                var zip = Enumerable.Zip(correctWords, answerWords, (x, y) => new { correctAnswer = x, answer = y });

                var result = zip.All(item => Compare(item.correctAnswer, item.answer, permissibleError));

                return result;
            }
        }

        private static bool Compare(string correctAnswer, string answer, double permissibleError)
        {
            var correctLength = correctAnswer.Length;

            if (correctLength <= 1)
            {
                var result = correctAnswer == answer;

                return result;
            }
            else
            {
                var distance = Levenshtein.CalculateDistance(correctAnswer, answer, 1);

                var minDistance = correctLength * permissibleError;

                var correctedMinDistance = Math.Max(minDistance, 1);

                var result = distance <= correctedMinDistance;

                return result;
            }
        }
    }
}
