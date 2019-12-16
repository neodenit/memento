using Neodenit.Memento.Common.Enums;

namespace Neodenit.Memento.Interfaces
{
    public interface IEvaluatorService
    {
        Mark Evaluate(string correctAnswer, string answer);
    }
}