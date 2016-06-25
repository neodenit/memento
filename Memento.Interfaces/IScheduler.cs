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
        void AddCloze(IDeck deck, ICollection<ICloze> clozes, ICloze cloze, string username);
        void MoveCloze(IEnumerable<ICloze> clozes, int oldPosition, int newPosition, int newDelay, bool correctMovedClozesDelays, bool correctRestClozesDelays, string username);
        void PrepareForAdding(IDeck deck, IEnumerable<ICloze> clozes, ICloze cloze, string username);
        void PrepareForRemoving(IDeck deck, IEnumerable<ICloze> clozes, ICloze cloze, string username);
        void PromoteCloze(IDeck deck, IEnumerable<ICloze> clozes, Delays delay, string username);
        void ShuffleNewClozes(IEnumerable<ICloze> clozes, string username);
    }
}