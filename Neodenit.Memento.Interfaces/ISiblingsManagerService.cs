using System.Collections.Generic;
using Neodenit.Memento.Models.DataModels;

namespace Neodenit.Memento.Interfaces
{
    public interface ISiblingsManagerService
    {
        void RearrangeSiblings(Deck deck, IEnumerable<UserRepetition> repetitions);
    }
}