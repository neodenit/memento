using Memento.Core;
using Memento.DomainModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.DomainModel.Repository
{
    public class EFMementoRepository : IMementoRepository
    {
        private MementoContext db = new MementoContext();

        public IQueryable<Deck> GetUserDecks(string userName)
        {
            return db.Decks.Where(item => item.Owner == userName);
        }

        public Task<Deck> FindDeckAsync(int? id)
        {
            return db.Decks.FindAsync(id);
        }

        public Task<Card> FindCardAsync(int? id)
        {
            return db.Cards.FindAsync(id);
        }

        public Task<Cloze> FindClozeAsync(int? id)
        {
            return db.Clozes.FindAsync(id);
        }

        public void AddDeck(Deck deck)
        {
            db.Decks.Add(deck);
        }

        public void AddCard(Card card)
        {
            db.Cards.Add(card);
        }

        public void AddCloze(Cloze cloze)
        {
            db.Clozes.Add(cloze);
        }
                
        public void RemoveDeck(Deck deck)
        {
            db.Decks.Remove(deck);
        }

        public void RemoveCard(Card card)
        {
            db.Cards.Remove(card);
        }

        public void RemoveCloze(Cloze cloze)
        {
            db.Clozes.Remove(cloze);
        }

        public void AddClozes(Card card, IEnumerable<string> clozeNames)
        {
            foreach (var clozeName in clozeNames)
            {
                var cloze = new Cloze(card.ID, clozeName);

                Scheduler.PrepareForAdding(card.Deck, card.Clozes, cloze);

                AddCloze(cloze);
            }
        }

        public void RemoveClozes(Card card, IEnumerable<string> clozeNames)
        {
            foreach (var clozeName in clozeNames)
            {
                var cloze = card.Clozes.Single(item => item.Label == clozeName);

                Scheduler.PrepareForRemoving(card.Deck, card.Clozes, cloze);

                card.Clozes.Remove(cloze);
            }
        }

        public void AddAnswer(Cloze cloze, bool isCorrect)
        {
            var card = cloze.Card;
            var deck = card.Deck;

            var answer = new Answer
            {
                Time = DateTime.Now,
                Owner = deck.Owner,
                ClozeID = cloze.ID,
                CardID = card.ID,
                DeckID = deck.ID,
                IsCorrect = isCorrect,
                CardsInRepetition = deck.GetClozes().Count(item => !item.IsNew)
            };

            db.Answers.Add(answer);
        }

        public void PromoteCard(Deck deck, Scheduler.Delays delay)
        {
            var clozes = deck.GetClozes();

            if (Settings.Default.EnableSiblingsHandling)
            {
                SiblingsManager.RearrangeSiblings(deck, clozes);
            }

            if (Settings.Default.EnableNewCardsHandling)
            {
                NewCardsManager.RearrangeNewCards(deck, clozes);
            }

            Scheduler.PromoteCard(deck, clozes, delay);
        }
        
        public Task SaveChangesAsync()
        {
            return db.SaveChangesAsync();
        }

        public void Dispose()
        {
            db.Dispose();
        }

        private IQueryable<Card> GetUserCards(string userName)
        {
            return db.Cards.Where(item => item.Deck.Owner == userName);
        }

        private IQueryable<Cloze> GetUserClozes(string userName)
        {
            return db.Clozes.Where(item => item.Card.Deck.Owner == userName);
        }

        private IQueryable<Answer> GetUserAnswers(string userName)
        {
            return db.Answers.Where(item => item.Owner == userName);
        }

        private void DeleteUserDecks(string userName)
        {
            var items = GetUserDecks(userName);
            db.Decks.RemoveRange(items);
            db.SaveChanges();
        }

        private void DeleteUserCards(string userName)
        {
            var items = GetUserCards(userName);
            db.Cards.RemoveRange(items);
            db.SaveChanges();
        }

        private void DeleteUserClozes(string userName)
        {
            var items = GetUserClozes(userName);
            db.Clozes.RemoveRange(items);
            db.SaveChanges();
        }

        public IQueryable<Answer> GetAnswersForDeck(int deckID)
        {
            return from answer in db.Answers where answer.DeckID == deckID select answer;
        }
    }
}
