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
        private readonly List<Deck> decks;
        private readonly List<Card> cards;
        private readonly List<Cloze> clozes;
        private readonly List<Answer> answers;

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

            this.decks = new List<Deck>(decks);
            this.cards = new List<Card>(cards);
            this.clozes = new List<Cloze>(clozes);
            this.answers = new List<Answer>(answers);
        }

        public void AddAnswer(Cloze cloze, bool isCorrect)
        {
            var answer = new Answer { ClozeID = cloze.ID, IsCorrect = isCorrect };
            answers.Add(answer);
        }

        public void AddCard(Card card) => cards.Add(card);

        public void AddCloze(Cloze cloze) => clozes.Add(cloze);

        public Task AddClozesAsync(Card card, IEnumerable<string> clozeNames) =>
            Task.Run(() =>
                {
                    foreach (var clozeName in clozeNames)
                    {
                        var cloze = new Cloze(card.ID, clozeName);
                        card.AddCloze(cloze);
                    }
                });

        public void AddDeck(Deck deck) => decks.Add(deck);

        public void Dispose() { }

        public Card FindCard(Guid id) =>
            cards.FirstOrDefault(c => c.ID == id);

        public Task<Card> FindCardAsync(Guid id) =>
            Task.FromResult(cards.FirstOrDefault(c => c.ID == id));

        public Cloze FindCloze(int id) =>
            clozes.FirstOrDefault(c => c.ID == id);

        public Task<Cloze> FindClozeAsync(int id) =>
            Task.FromResult(clozes.FirstOrDefault(c => c.ID == id));

        public Deck FindDeck(int id) =>
            decks.FirstOrDefault(d => d.ID == id);

        public Task<Deck> FindDeckAsync(int id) =>
            Task.FromResult(decks.FirstOrDefault(d => d.ID == id));

        public Task<IEnumerable<Answer>> GetAnswersForDeckAsync(int deckID) =>
            Task.FromResult(answers.Where(a => a.DeckID == deckID));

        public Task<IEnumerable<Deck>> GetUserDecksAsync(string userName) =>
            Task.FromResult(decks.Where(d => d.Owner == userName  && !d.IsShared));

        public Task<IEnumerable<Deck>> GetSharedDecksAsync() =>
            Task.FromResult(decks.Where(d => d.IsShared));

        public void PromoteCloze(Deck deck, Delays delay, string username) { }

        public void RemoveCard(Card card) => cards.Remove(card);

        public void RemoveCloze(Cloze cloze) => clozes.Remove(cloze);

        public void RemoveClozes(Card card, IEnumerable<string> clozeNames)
        {
            foreach (var clozeName in clozeNames)
            {
                var cloze = card.GetClozes().FirstOrDefault(c => c.Label == clozeName);
                clozes.Remove(cloze);
            }
        }

        public void RemoveDeck(Deck deck) => decks.Remove(deck);

        public Task SaveChangesAsync() => Task.Delay(1);
    }
}
