using System;
using System.Collections.Generic;
using System.Linq;
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

        public Task AddClozesAsync(ICard card, IEnumerable<string> clozeNames) =>
            Task.Run(() =>
                {
                    foreach (var clozeName in clozeNames)
                    {
                        var cloze = new Cloze(card.ID, clozeName);
                        card.AddCloze(cloze);
                    }
                });

        public void AddDeck(IDeck deck) => decks.Add(deck);

        public void Dispose() { }

        public ICard FindCard(Guid id) =>
            cards.FirstOrDefault(c => c.ID == id);

        public Task<ICard> FindCardAsync(Guid id) =>
            Task.FromResult(cards.FirstOrDefault(c => c.ID == id));

        public ICloze FindCloze(int id) =>
            clozes.FirstOrDefault(c => c.ID == id);

        public Task<ICloze> FindClozeAsync(int id) =>
            Task.FromResult(clozes.FirstOrDefault(c => c.ID == id));

        public IDeck FindDeck(int id) =>
            decks.FirstOrDefault(d => d.ID == id);

        public Task<IDeck> FindDeckAsync(int id) =>
            Task.FromResult(decks.FirstOrDefault(d => d.ID == id));

        public Task<IEnumerable<IAnswer>> GetAnswersForDeckAsync(int deckID) =>
            Task.FromResult(answers.Where(a => a.DeckID == deckID));

        public Task<IEnumerable<IDeck>> GetUserDecksAsync(string userName) =>
            Task.FromResult(decks.Where(d => d.Owner == userName  && !d.IsShared));

        public Task<IEnumerable<IDeck>> GetSharedDecksAsync() =>
            Task.FromResult(decks.Where(d => d.IsShared));

        public void PromoteCloze(IDeck deck, Delays delay, string username) { }

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
