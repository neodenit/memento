using Memento.Interfaces;
using Memento.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Services
{
    public class DecksService : IDecksService
    {
        private readonly IMementoRepository repository;

        public DecksService(IMementoRepository repository)
        {
            this.repository = repository;
        }

        public async Task<IEnumerable<IDeck>> GetDecksAsync(string username)
        {
            var decks = await repository.GetUserDecksAsync(username);
            var orderedDecks = decks.OrderBy(deck => deck.Title);
            return orderedDecks;
        }

        public async Task<IEnumerable<IDeck>> GetSharedDecksAsync()
        {
            var decks = await repository.GetSharedDecksAsync();
            var orderedDecks = decks.OrderBy(deck => deck.Title);
            return orderedDecks;
        }

        public async Task<IDeckWithStatViewModel> GetDeckWithStatViewModel(int deckID, IStatistics statistics)
        {
            var deck = await repository.FindDeckAsync(deckID);

            var clozes = deck.GetClozes();
            statistics.NewQuestionsCount = clozes.Count(c => c.IsNew);
            statistics.OldQuestionsCount = clozes.Count(c => !c.IsNew);

            var viewModel = new DeckWithStatViewModel
            {
                Deck = deck,
                Stat = statistics,
            };

            return viewModel;
        }

        public Task<IDeck> FindDeckAsync(int id) =>
            repository.FindDeckAsync(id);

        public async Task CreateDeck(IDeck deck, string userName)
        {
            deck.Owner = userName;

            repository.AddDeck(deck);

            await repository.SaveChangesAsync();
        }

        public async Task UpdateDeck(int id, string title, int startDelay, double coeff)
        {
            var dbDeck = await repository.FindDeckAsync(id);

            dbDeck.Title = title;
            dbDeck.StartDelay = startDelay;
            dbDeck.Coeff = coeff;

            await repository.SaveChangesAsync();
        }

        public async Task DeleteDeck(int id)
        {
            var deck = await repository.FindDeckAsync(id);

            repository.RemoveDeck(deck);

            await repository.SaveChangesAsync();
        }

        public async Task ShareDeckAsync(int id)
        {
            var deck = await repository.FindDeckAsync(id);

            deck.IsShared = true;

            await repository.SaveChangesAsync();
        }
    }
}
