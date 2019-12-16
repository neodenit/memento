using System.Linq;
using Neodenit.Memento.Common.Enums;

namespace Neodenit.Memento.Services.Evaluators
{
    public class PhraseEvaluatorService : BaseEvaluatorService
    {
        public override Mark Evaluate(string correctAnswer, string answer)
        {
            var correctAnswerVariants = GetVariants(correctAnswer);

            var correctVariantsWords = from item in correctAnswerVariants select Normalize(item);

            var answerWords = Normalize(answer);

            var marks = from item in correctVariantsWords select Check(item, answerWords);

            return GetBestMark(marks);
        }
    }
}
