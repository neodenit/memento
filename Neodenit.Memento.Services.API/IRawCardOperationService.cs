using System;
using System.Collections.Generic;

namespace Neodenit.Memento.Services.API
{
    public interface IRawCardOperationService
    {
        IEnumerable<string> GetCards(string deckText);

        IEnumerable<Tuple<string, string>> RawCardsToClozes(IEnumerable<string> cards);
    }
}