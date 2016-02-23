using Memento.DomainModel.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Memento.DomainModel.Models;
using Memento.Interfaces;

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
            this.decks = new List<Deck>(decks);
            this.cards = new List<Card>(cards);
            this.clozes = new List<Cloze>(clozes);
            this.answers = new List<Answer>(answers);

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
        }

        public void AddAnswer(Cloze cloze, bool isCorrect)
        {
            var answer = new Answer { ClozeID = cloze.ID, IsCorrect = isCorrect };
            answers.Add(answer);
        }

        public void AddCard(Card card) => cards.Add(card);

        public void AddCloze(Cloze cloze) => clozes.Add(cloze);

        public void AddClozes(Card card, IEnumerable<string> clozeNames)
        {
            foreach (var clozeName in clozeNames)
            {
                var cloze = new Cloze(card.ID, clozeName);
                card.Clozes.Add(cloze);
            }
        }

        public void AddDeck(Deck deck) => decks.Add(deck);

        public void Dispose() { }

        public Card FindCard(int? id) =>
            cards.FirstOrDefault(c => c.ID == id.Value);

        public Task<Card> FindCardAsync(int? id) =>
            Task.FromResult(cards.FirstOrDefault(c => c.ID == id.Value));

        public Cloze FindCloze(int? id) =>
            clozes.FirstOrDefault(c => c.ID == id.Value);

        public Task<Cloze> FindClozeAsync(int? id) =>
            Task.FromResult(clozes.FirstOrDefault(c => c.ID == id.Value));

        public Deck FindDeck(int? id) =>
            decks.FirstOrDefault(d => d.ID == id.Value);

        public Task<Deck> FindDeckAsync(int? id) =>
            Task.FromResult(decks.FirstOrDefault(d => d.ID == id.Value));

        public IQueryable<Answer> GetAnswersForDeck(int deckID) =>
            answers.Where(a => a.DeckID == deckID).AsQueryable();

        public IQueryable<Deck> GetUserDecks(string userName) =>
            decks.Where(d => d.Owner == userName).AsQueryable();

        public void PromoteCard(Deck deck, Delays delay) { }

        public void RemoveCard(Card card) => cards.Remove(card);

        public void RemoveCloze(Cloze cloze) => clozes.Remove(cloze);

        public void RemoveClozes(Card card, IEnumerable<string> clozeNames)
        {
            foreach (var clozeName in clozeNames)
            {
                var cloze = card.Clozes.FirstOrDefault(c => c.Label == clozeName);
                card.Clozes.Remove(cloze);
            }
        }

        public void RemoveDeck(Deck deck) => decks.Remove(deck);

        public Task SaveChangesAsync() => Task.Delay(1);
    }
}
