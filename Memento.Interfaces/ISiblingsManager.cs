using System.Collections.Generic;

namespace Memento.Interfaces
{
    public interface ISiblingsManager
    {
        void RearrangeSiblings(IDeck deck, IEnumerable<ICard> cards);
    }
}