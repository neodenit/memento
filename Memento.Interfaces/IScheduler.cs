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
        void AddCard(IDeck deck, ICollection<ICard> cards, ICard card);
        void MoveCard(IEnumerable<ICard> cards, int oldPosition, int newPosition, int newDelay, bool correctMovedCardsDelays, bool correctRestCardsDelays);
        void PrepareForAdding(IDeck deck, IEnumerable<ICard> cards, ICard card);
        void PrepareForRemoving(IDeck deck, IEnumerable<ICard> cards, ICard card);
        void PromoteCard(IDeck deck, IEnumerable<ICard> cards, Delays delay);
        void ShuffleNewCards(IEnumerable<ICard> cards);
    }
}