using System.Collections.Generic;
using Neodenit.Memento.Models.DataModels;

namespace Neodenit.Memento.Interfaces
{
    public interface INewClozesManagerService
    {
        void RearrangeNewRepetitions(Deck deck, IEnumerable<UserRepetition> repetitions);
    }
}