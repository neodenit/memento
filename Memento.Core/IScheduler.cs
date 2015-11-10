using System.Collections.Generic;

namespace Memento.Core
{
    public interface IScheduler
    {
        void AddCard(IDeck deck, ICollection<ICard> cards, ICard card);
        void MoveCard(IEnumerable<ICard> cards, int oldPosition, int newPosition, int newDelay, bool correctMovedCardsDelays, bool correctRestCardsDelays);
        void PrepareForAdding(IDeck deck, IEnumerable<ICard> cards, ICard card);
        void PrepareForRemoving(IDeck deck, IEnumerable<ICard> cards, ICard card);
        void PromoteCard(IDeck deck, IEnumerable<ICard> cards, Scheduler.Delays delay);
        void ShuffleNewCards(IEnumerable<ICard> cards);
    }
}