using Memento.Models.Enums;

namespace Memento.Interfaces
{
    public interface IEvaluatorService
    {
        Mark Evaluate(string correctAnswer, string answer);
    }
}