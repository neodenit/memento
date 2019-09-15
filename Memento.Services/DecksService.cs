﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memento.Additional;
using Memento.Interfaces;
using Memento.Models.Models;
using Memento.Models.ViewModels;

namespace Memento.Services
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

        public async Task<DeckWithStatViewModel> GetDeckWithStatViewModel(int deckID, Statistics statistics, string username)
        {
            var deck = await repository.FindDeckAsync(deckID);

            var clozes = deck.GetClozes();
            statistics.NewQuestionCount = clozes.Count(c => c.GetUserRepetition(username).IsNew);
            statistics.OldQuestionCount = clozes.Count(c => !c.GetUserRepetition(username).IsNew);

            var viewModel = new DeckWithStatViewModel
            {
                Deck = deck,
                Stat = statistics,
            };

            return viewModel;
        }

        public Task<Deck> FindDeckAsync(int id) =>
            repository.FindDeckAsync(id);

        public async Task CreateDeck(Deck deck, string userName)
        {
            deck.Owner = userName;

            repository.AddDeck(deck);

            await repository.SaveChangesAsync();
        }

        public async Task UpdateDeck(int id, string title, int startDelay, double coeff, bool previewAnswer)
        {
            var dbDeck = await repository.FindDeckAsync(id);

            dbDeck.Title = title;
            dbDeck.StartDelay = startDelay;
            dbDeck.Coeff = coeff;
            dbDeck.PreviewAnswer = previewAnswer;

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
