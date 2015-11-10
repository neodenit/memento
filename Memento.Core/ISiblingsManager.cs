using System.Collections.Generic;

namespace Memento.Core
{
    public interface ISiblingsManager
    {
        void RearrangeSiblings(IDeck deck, IEnumerable<ICard> cards);
    }
}