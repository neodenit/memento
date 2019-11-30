using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Memento.Common;
using Memento.Interfaces;
using Memento.Models.Enums;
using MinimumEditDistance;

namespace Memento.Services.Evaluators
{
    public abstract class BaseEvaluatorService : IEvaluatorService
    {
        public abstract Mark Evaluate(string correctAnswer, string answer);

        protected string Normalize(string answer)
        {
            var words = GetWords(answer);

            return string.Join(" ", words);
        }

        protected IEnumerable<string> GetVariants(string correctAnswer)
        {
            return correctAnswer.Split('|');
        }

        protected IEnumerable<string> GetWords(string correctAnswer) =>
            from word in Regex.Split(correctAnswer, @"\W+")
            where !string.IsNullOrEmpty(word)
            select word.ToUpper();

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
                var permissibleError = Settings.Default.PermissibleError;

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
