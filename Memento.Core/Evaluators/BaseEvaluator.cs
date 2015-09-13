using MinimumEditDistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Memento.Core.Evaluators
{
    public enum Mark
    {
        Correct,
        Incorrect,
        Typo,
    }

    public abstract class BaseEvaluator
    {
        protected double permissibleError;

        public BaseEvaluator(double permissibleError)
        {
            this.permissibleError = permissibleError;
        }

        public abstract Mark Evaluate(string correctAnswer, string answer);

        protected string Normalize(string answer)
        {
            var words = GetWords(answer);

            return String.Join(" ", words);
        }

        protected IEnumerable<string> GetVariants(string correctAnswer)
        {
            return correctAnswer.Split('|');
        }

        protected IEnumerable<string> GetWords(string correctAnswer)
        {
            var words = from word in Regex.Split(correctAnswer, @"\W") select word.ToUpper();

            return words;
        }

        protected Mark Check(IEnumerable<string> correctWords, IEnumerable<string> answerWords)
        {
            if (correctWords.Count() != answerWords.Count())
            {
                return Mark.Incorrect;
            }
            else
            {
                var zip = Enumerable.Zip(correctWords, answerWords, (x, y) => new { correctAnswer = x, answer = y });

                var marks = from item in zip select Check(item.correctAnswer, item.answer);

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

        protected Mark Check(string correctAnswer, string answer)
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

        protected Mark GetBestMark(IEnumerable<Mark> marks)
        {
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
    }
}
