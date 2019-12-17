using Neodenit.Memento.Common.Enums;

namespace Neodenit.Memento.Services.API
{
    public interface IEvaluatorService
    {
        Mark Evaluate(string correctAnswer, string answer);
    }
}