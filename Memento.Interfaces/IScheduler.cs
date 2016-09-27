using System.Collections.Generic;

namespace Memento.Interfaces
{
    public enum Delays
    {
        AfterNext,
        Next,
        Same,
        Previous,
        Initial,
    }

    public interface IScheduler
    {
        void MoveRepetition(IEnumerable<IUserRepetition> repetitions, int oldPosition, int newPosition, int newDelay, bool correctMovedRepetitionsDelays, bool correctRestRepetitionsDelays);
        void PrepareForAdding(IDeck deck, IEnumerable<IUserRepetition> repetitions, IUserRepetition repetition);
        void PrepareForRemoving(IDeck deck, IEnumerable<IUserRepetition> repetitions, IUserRepetition repetition);
        void PromoteRepetition(IDeck deck, IEnumerable<IUserRepetition> repetitions, Delays delay);
        void ShuffleNewRepetitions(IEnumerable<IUserRepetition> repetitions);
    }
}