﻿using Memento.DomainModel.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Memento.Interfaces;
using Memento.Models.Models;

namespace Memento.Tests
{
    public class FakeRepository : IMementoRepository
    {
        private readonly List<IDeck> decks;
        private readonly List<ICard> cards;
        private readonly List<ICloze> clozes;
        private readonly List<IAnswer> answers;

        public FakeRepository(IEnumerable<Deck> decks, IEnumerable<Card> cards, IEnumerable<Cloze> clozes, IEnumerable<Answer> answers)
        {
            foreach (var deck in decks)
            {
                foreach (var card in deck.Cards)
                {
                    card.DeckID = deck.ID;
                    card.Deck = deck;
                }
            }

            foreach (var card in cards)
            {
                foreach (var cloze in card.Clozes)
                {
                    cloze.CardID = card.ID;
                    cloze.Card = card;
                }
            }

            this.decks = new List<IDeck>(decks);
            this.cards = new List<ICard>(cards);
            this.clozes = new List<ICloze>(clozes);
            this.answers = new List<IAnswer>(answers);
        }

        public void AddAnswer(ICloze cloze, bool isCorrect)
        {
            var answer = new Answer { ClozeID = cloze.ID, IsCorrect = isCorrect };
            answers.Add(answer);
        }

        public void AddCard(ICard card) => cards.Add(card);

        public void AddCloze(ICloze cloze) => clozes.Add(cloze);

        public void AddClozes(ICard card, IEnumerable<string> clozeNames)
        {
            foreach (var clozeName in clozeNames)
            {
                var cloze = new Cloze(card.ID, clozeName);
                card.AddCloze(cloze);
            }
        }

        public void AddDeck(IDeck deck) => decks.Add(deck);

        public void Dispose() { }

        public ICard FindCard(int? id) =>
            cards.FirstOrDefault(c => c.ID == id.Value);

        public Task<ICard> FindCardAsync(int? id) =>
            Task.FromResult(cards.FirstOrDefault(c => c.ID == id.Value));

        public ICloze FindCloze(int? id) =>
            clozes.FirstOrDefault(c => c.ID == id.Value);

        public Task<ICloze> FindClozeAsync(int? id) =>
            Task.FromResult(clozes.FirstOrDefault(c => c.ID == id.Value));

        public IDeck FindDeck(int? id) =>
            decks.FirstOrDefault(d => d.ID == id.Value);

        public Task<IDeck> FindDeckAsync(int? id) =>
            Task.FromResult(decks.FirstOrDefault(d => d.ID == id.Value));

        public IQueryable<IAnswer> GetAnswersForDeck(int deckID) =>
            answers.Where(a => a.DeckID == deckID).AsQueryable();

        public IQueryable<IDeck> GetUserDecks(string userName) =>
            decks.Where(d => d.Owner == userName).AsQueryable();

        public void PromoteCard(IDeck deck, Delays delay) { }

        public void RemoveCard(ICard card) => cards.Remove(card);

        public void RemoveCloze(ICloze cloze) => clozes.Remove(cloze);

        public void RemoveClozes(ICard card, IEnumerable<string> clozeNames)
        {
            foreach (var clozeName in clozeNames)
            {
                var cloze = card.GetClozes().FirstOrDefault(c => c.Label == clozeName);
                clozes.Remove(cloze);
            }
        }

        public void RemoveDeck(IDeck deck) => decks.Remove(deck);

        public Task SaveChangesAsync() => Task.Delay(1);
    }
}
