using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Neodenit.Memento.Common;
using Neodenit.Memento.Common.Enums;
using Neodenit.Memento.DataAccess.API;
using Neodenit.Memento.DataAccess.API.DataModels;
using Neodenit.Memento.Interfaces;
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

        public async Task<IEnumerable<DeckViewModel>> GetDecksAsync(string userName)
        {
            var decks = await repository.GetUserDecksAsync(userName);
            var orderedDecks = decks.OrderBy(deck => deck.Title);

            var viewModel = mapper.Map<IEnumerable<DeckViewModel>>(decks);
            return viewModel;
        }

        public async Task<IEnumerable<DeckViewModel>> GetSharedDecksAsync()
        {
            var decks = await repository.GetSharedDecksAsync();
            var orderedDecks = decks.OrderBy(deck => deck.Title);

            var viewModel = mapper.Map<IEnumerable<DeckViewModel>>(decks);
            return viewModel;
        }

        public async Task<DeckWithStatViewModel> GetDeckWithStatViewModel(Guid deckId, StatisticsViewModel statistics, string username)
        {
            var deck = await repository.FindDeckAsync(deckId);

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

        public async Task<DeckViewModel> FindDeckAsync(Guid id)
        {
            var deck = await repository.FindDeckAsync(id);

            var viewModel = mapper.Map<DeckViewModel>(deck);
            return viewModel;
        }

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
            var deck = await repository.FindDeckAsync(id);

            deck.Title = title;
            deck.StartDelay = startDelay;
            deck.Coeff = coeff;
            deck.PreviewAnswer = previewAnswer;

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

        public async Task<IEnumerable<ClozeViewModel>> GetClozesAsync(Guid deckId, string userName)
        {
            var deck = await repository.FindDeckAsync(deckId);
            var clozes = deck.GetClozes();
            var orderedClozes = clozes.OrderBy(cloze => cloze.GetUserRepetition(userName).Position);

            var viewModel = from cloze in orderedClozes
                            select mapper.Map<Cloze, ClozeViewModel>(cloze, opt =>
                                opt.AfterMap((src, dest) =>
                                {
                                    var repetition = cloze.GetUserRepetition(userName);

                                    dest.Position = repetition.Position;
                                    dest.IsNew = repetition.IsNew;
                                    dest.LastDelay = repetition.LastDelay;
                                }));

            return viewModel;
        }

        public async Task<IEnumerable<ViewCardViewModel>> GetCardsAsync(Guid deckId)
        {
            var deck = await repository.FindDeckAsync(deckId);
            var cards = deck.GetValidCards();

            var viewModel = mapper.Map<IEnumerable<ViewCardViewModel>>(cards);
            return viewModel;
        }

        public async Task<IEnumerable<ViewCardViewModel>> GetDeletedCardsAsync(Guid deckId)
        {
            var deck = await repository.FindDeckAsync(deckId);
            var cards = deck.GetDeletedCards();

            var viewModel = mapper.Map<IEnumerable<ViewCardViewModel>>(cards);
            return viewModel;
        }

        public async Task<IEnumerable<ViewCardViewModel>> GetDraftCardsAsync(Guid deckId)
        {
            var deck = await repository.FindDeckAsync(deckId);
            var cards = deck.GetDraftCards();

            var viewModel = mapper.Map<IEnumerable<ViewCardViewModel>>(cards);
            return viewModel;
        }
    }
}
