using System.Linq;
using Memento.Models.Enums;

namespace Memento.Core.Evaluators
{
    public class WordsEvaluator : BaseEvaluator
    {
        public override Mark Evaluate(string correctAnswer, string answer)
        {
            var correctAnswerVariants = GetVariants(correctAnswer);

            var correctVariantsWords = from item in correctAnswerVariants select GetWords(item);

            var answerWords = GetWords(answer);

            var marks = from item in correctVariantsWords select Check(item, answerWords);

            return GetBestMark(marks);
        }
    }
}
