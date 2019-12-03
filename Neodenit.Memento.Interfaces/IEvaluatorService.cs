using Neodenit.Memento.Models.Enums;

namespace Neodenit.Memento.Interfaces
{
    public interface IEvaluatorService
    {
        Mark Evaluate(string correctAnswer, string answer);
    }
}