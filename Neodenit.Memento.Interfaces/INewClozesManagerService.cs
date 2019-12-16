using System.Collections.Generic;
using Neodenit.Memento.DataAccess.API.DataModels;

namespace Neodenit.Memento.Interfaces
{
    public interface INewClozesManagerService
    {
        void RearrangeNewRepetitions(Deck deck, IEnumerable<UserRepetition> repetitions);
    }
}