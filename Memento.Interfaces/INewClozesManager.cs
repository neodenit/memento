using System.Collections.Generic;
using Memento.Models.Models;

namespace Memento.Interfaces
{
    public interface INewClozesManager
    {
        void RearrangeNewRepetitions(Deck deck, IEnumerable<UserRepetition> repetitions);
    }
}