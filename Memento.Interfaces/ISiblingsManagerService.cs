using System.Collections.Generic;
using Memento.Models.Models;

namespace Memento.Interfaces
{
    public interface ISiblingsManagerService
    {
        void RearrangeSiblings(Deck deck, IEnumerable<UserRepetition> repetitions);
    }
}