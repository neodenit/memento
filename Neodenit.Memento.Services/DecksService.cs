using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neodenit.Memento.Interfaces;
using Neodenit.Memento.Models.DataModels;
using Neodenit.Memento.Models.ViewModels;

namespace Neodenit.Memento.Services
{
    public class DecksService : IDecksService
    {
        private readonly IMementoRepository repository;

        public DecksService(IMementoRepository repository)
        {
            this.repository = repository;
        }

        public async Task<IEnumerable<Deck>> GetDecksAsync(string username)
        {
            var decks = await repository.GetUserDecksAsync(username);
            var orderedDecks = decks.OrderBy(deck => deck.Title);
            return orderedDecks;
        }

        public async Task<IEnumerable<Deck>> GetSharedDecksAsync()
        {
            var decks = await repository.GetSharedDecksAsync();
            var orderedDecks = decks.OrderBy(deck => deck.Title);
            return orderedDecks;
        }

        public async Task<DeckWithStatViewModel> GetDeckWithStatViewModel(Guid deckID, StatisticsViewModel statistics, string username)
        {
            var deck = await repository.FindDeckAsync(deckID);

            var clozes = deck.GetClozes();
            statistics.NewQuestionCount = clozes.Count(c => c.GetUserRepetition(username).IsNew);
            statistics.OldQuestionCount = clozes.Count(c => !c.GetUserRepetition(username).IsNew);

            var viewModel = new DeckWithStatViewModel
            {
                Deck = new DeckViewModel
                {
                    ID = deck.ID,
                    Title = deck.Title,
                    ControlMode = deck.ControlMode,
                    DelayMode = deck.DelayMode,
                    StartDelay = deck.StartDelay,
                    Coeff = deck.Coeff,
                    FirstDelay = deck.StartDelay,
                    SecondDelay = (int)Math.Round(deck.StartDelay * deck.Coeff),
                    PreviewAnswer = deck.PreviewAnswer
                },
                Stat = statistics
            };

            return viewModel;
        }

        public Task<Deck> FindDeckAsync(Guid id) =>
            repository.FindDeckAsync(id);

        public async Task CreateDeck(Deck deck, string userName)
        {
            deck.Owner = userName;
            deck.ID = Guid.NewGuid();

            repository.AddDeck(deck);

            await repository.SaveChangesAsync();
        }

        public async Task UpdateDeck(Guid id, string title, int startDelay, double coeff, bool previewAnswer)
        {
            var dbDeck = await repository.FindDeckAsync(id);

            dbDeck.Title = title;
            dbDeck.StartDelay = startDelay;
            dbDeck.Coeff = coeff;
            dbDeck.PreviewAnswer = previewAnswer;

            await repository.SaveChangesAsync();
        }

        public async Task DeleteDeck(Guid id)
        {
            var deck = await repository.FindDeckAsync(id);

            repository.RemoveDeck(deck);

            await repository.SaveChangesAsync();
        }

        public async Task ShareDeckAsync(Guid id)
        {
            var deck = await repository.FindDeckAsync(id);

            deck.IsShared = true;

            await repository.SaveChangesAsync();
        }
    }
}
