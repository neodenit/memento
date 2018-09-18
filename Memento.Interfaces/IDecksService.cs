using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface IDecksService
    {
        Task<IEnumerable<IDeck>> GetDecksAsync(string username);
        Task<IEnumerable<IDeck>> GetSharedDecksAsync();
        Task<IDeckWithStatViewModel> GetDeckWithStatViewModel(int deckID, IStatistics statistics, string username);
        Task<IDeck> FindDeckAsync(int id);
        Task UpdateDeck(int id, string title, int startDelay, double coeff, bool previewAnswer);
        Task CreateDeck(IDeck deck, string userName);
        Task DeleteDeck(int id);
        Task ShareDeckAsync(int id);
    }
}