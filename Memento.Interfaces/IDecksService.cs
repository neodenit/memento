using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface IDecksService
    {
        Task<IEnumerable<IDeck>> GetDecksAsync(string username);
        Task<IDeckWithStatViewModel> GetDeckWithStatViewModel(int deckID, IStatistics statistics);
    }
}