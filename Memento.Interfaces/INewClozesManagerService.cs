using System.Collections.Generic;
using Memento.Models.Models;

namespace Memento.Interfaces
{
    public interface INewClozesManagerService
    {
        void RearrangeNewRepetitions(Deck deck, IEnumerable<UserRepetition> repetitions);
    }
}