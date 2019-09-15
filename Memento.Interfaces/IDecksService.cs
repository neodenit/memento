using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Memento.Additional;
using Memento.Models.Models;
using Memento.Models.ViewModels;

namespace Memento.Interfaces
{
    public interface IDecksService
    {
        Task<IEnumerable<Deck>> GetDecksAsync(string username);
        Task<IEnumerable<Deck>> GetSharedDecksAsync();
        Task<DeckWithStatViewModel> GetDeckWithStatViewModel(int deckID, Statistics statistics, string username);
        Task<Deck> FindDeckAsync(int id);
        Task UpdateDeck(int id, string title, int startDelay, double coeff, bool previewAnswer);
        Task CreateDeck(Deck deck, string userName);
        Task DeleteDeck(int id);
        Task ShareDeckAsync(int id);
    }
}