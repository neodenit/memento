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
        void AddCloze(IDeck deck, ICollection<ICloze> clozes, ICloze cloze);
        void MoveCloze(IEnumerable<ICloze> clozes, int oldPosition, int newPosition, int newDelay, bool correctMovedClozesDelays, bool correctRestClozesDelays);
        void PrepareForAdding(IDeck deck, IEnumerable<ICloze> clozes, ICloze cloze);
        void PrepareForRemoving(IDeck deck, IEnumerable<ICloze> clozes, ICloze cloze);
        void PromoteCloze(IDeck deck, IEnumerable<ICloze> clozes, Delays delay);
        void ShuffleNewClozes(IEnumerable<ICloze> clozes);
    }
}