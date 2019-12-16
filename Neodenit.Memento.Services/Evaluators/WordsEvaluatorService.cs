using System.Linq;
using Neodenit.Memento.Common.Enums;

namespace Neodenit.Memento.Services.Evaluators
{
    public class WordsEvaluatorService : BaseEvaluatorService
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
