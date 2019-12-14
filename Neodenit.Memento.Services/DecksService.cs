using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Neodenit.Memento.Common;
using Neodenit.Memento.Interfaces;
using Neodenit.Memento.Models.DataModels;
using Neodenit.Memento.Models.Enums;
using Neodenit.Memento.Models.ViewModels;

namespace Neodenit.Memento.Services
{
    public class DecksService : IDecksService
    {
        private readonly IMapper mapper;
        private readonly IMementoRepository repository;

        public DecksService(IMapper mapper, IMementoRepository repository)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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
                Deck = mapper.Map<DeckViewModel>(deck),
                Stat = statistics
            };

            return viewModel;
        }

        public Task<Deck> FindDeckAsync(Guid id) =>
            repository.FindDeckAsync(id);

        public async Task CreateDeck(DeckViewModel deck, string userName)
        {
            var newDeck = new Deck
            {
                ID = Guid.NewGuid(),
                Owner = userName,
                AllowSmallDelays = Settings.Default.AllowSmallDelays,
                Title = deck.Title,
                DelayMode = Settings.Default.AllowSmoothDelayModes
                    ? deck.DelayMode
                    : DelayModes.Sharp,
                ControlMode = deck.ControlMode,
                StartDelay = Settings.Default.EnableTwoStepsConfig
                    ? deck.FirstDelay
                    : deck.StartDelay,
                Coeff = Settings.Default.EnableTwoStepsConfig
                    ? (double)deck.SecondDelay / deck.FirstDelay
                    : deck.Coeff,
                PreviewAnswer = deck.PreviewAnswer
            };

            repository.AddDeck(newDeck);

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
