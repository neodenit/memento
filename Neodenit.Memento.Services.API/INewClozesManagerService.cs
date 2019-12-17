using System.Collections.Generic;
using Neodenit.Memento.Common.DataModels;

namespace Neodenit.Memento.Services.API
{
    public interface INewClozesManagerService
    {
        void RearrangeNewRepetitions(Deck deck, IEnumerable<UserRepetition> repetitions);
    }
}