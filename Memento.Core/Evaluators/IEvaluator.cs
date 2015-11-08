namespace Memento.Core.Evaluators
{
    public interface IEvaluator
    {
        Mark Evaluate(string correctAnswer, string answer);
    }
}