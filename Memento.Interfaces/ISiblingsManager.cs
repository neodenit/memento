using System.Collections.Generic;
using Memento.Models.Models;

namespace Memento.Interfaces
{
    public interface ISiblingsManager
    {
        void RearrangeSiblings(Deck deck, IEnumerable<UserRepetition> repetitions);
    }
}