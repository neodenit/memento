using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Core.Evaluators
{
    public class PhraseEvaluator : BaseEvaluator
    {
        public PhraseEvaluator(double permissibleError) : base(permissibleError)
        {
        }

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
