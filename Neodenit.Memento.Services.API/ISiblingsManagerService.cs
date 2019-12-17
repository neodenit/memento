using System.Collections.Generic;
using Neodenit.Memento.Common.DataModels;

namespace Neodenit.Memento.Services.API
{
    public interface ISiblingsManagerService
    {
        void RearrangeSiblings(Deck deck, IEnumerable<UserRepetition> repetitions);
    }
}