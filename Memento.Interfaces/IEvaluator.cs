using Memento.Models.Enums;

namespace Memento.Interfaces
{
    public interface IEvaluator
    {
        Mark Evaluate(string correctAnswer, string answer);
    }
}