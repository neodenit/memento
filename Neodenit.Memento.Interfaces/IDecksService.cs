using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neodenit.Memento.Models.ViewModels;

namespace Neodenit.Memento.Interfaces
{
    public interface IDecksService
    {
        Task<IEnumerable<DeckViewModel>> GetDecksAsync(string userName);

        Task<IEnumerable<DeckViewModel>> GetSharedDecksAsync();

        Task<DeckWithStatViewModel> GetDeckWithStatViewModel(Guid deckID, StatisticsViewModel statistics, string username);

        Task<DeckViewModel> FindDeckAsync(Guid id);

        DeckViewModel FindDeck(Guid id);

        Task UpdateDeck(Guid id, string title, int startDelay, double coeff, bool previewAnswer);

        Task CreateDeck(DeckViewModel deck, string userName);

        Task DeleteDeck(Guid id);

        Task ShareDeckAsync(Guid id);

        Task<IEnumerable<ClozeViewModel>> GetClozesAsync(Guid deckId, string userName);

        Task<IEnumerable<ViewCardViewModel>> GetCardsAsync(Guid deckId);

        Task<IEnumerable<ViewCardViewModel>> GetDeletedCardsAsync(Guid deckId);

        Task<IEnumerable<ViewCardViewModel>> GetDraftCardsAsync(Guid deckId);
    }
}