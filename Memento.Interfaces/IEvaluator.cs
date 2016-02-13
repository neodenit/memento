namespace Memento.Interfaces
{
    public enum Mark
    {
        Correct,
        Incorrect,
        Typo,
    }

    public interface IEvaluator
    {
        Mark Evaluate(string correctAnswer, string answer);
    }
}