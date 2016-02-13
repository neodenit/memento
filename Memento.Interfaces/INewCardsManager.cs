using System.Collections.Generic;

namespace Memento.Interfaces
{
    public interface INewCardsManager
    {
        void RearrangeNewCards(IDeck deck, IEnumerable<ICard> cards);
    }
}