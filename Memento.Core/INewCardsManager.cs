using System.Collections.Generic;

namespace Memento.Core
{
    public interface INewCardsManager
    {
        void RearrangeNewCards(IDeck deck, IEnumerable<ICard> cards);
    }
}