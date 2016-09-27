using Memento.Common;
using Memento.Interfaces;
using Memento.Models.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.DomainModel.Repository
{
    public class EFMementoRepository : IMementoRepository, IDisposable
    {
        private MementoContext db = new MementoContext();
        private readonly IScheduler scheduler;
        private readonly ISiblingsManager siblingsManager;
        private readonly INewClozesManager newCardsManager;

        public EFMementoRepository(IScheduler scheduler, ISiblingsManager siblingsManager, INewClozesManager newCardsManager)
        {
            this.scheduler = scheduler;
            this.siblingsManager = siblingsManager;
            this.newCardsManager = newCardsManager;
        }

        public async Task<IEnumerable<IDeck>> GetSharedDecksAsync() =>
            await db.Decks.Where(item => item.IsShared).ToListAsync();

        public async Task<IEnumerable<IDeck>> GetUserDecksAsync(string userName) =>
            await db.Decks.Where(item => item.Owner == userName && !item.IsShared).ToListAsync();

        public IDeck FindDeck(int id) =>
            db.Decks.Find(id);

        public ICard FindCard(int id) =>
            db.Cards.Find(id);

        public ICloze FindCloze(int id) =>
            db.Clozes.Find(id);

        public async Task<IDeck> FindDeckAsync(int id) =>
            await db.Decks.FindAsync(id);

        public async Task<ICard> FindCardAsync(int id) =>
            await db.Cards.FindAsync(id);

        public async Task<ICloze> FindClozeAsync(int id) =>
            await db.Clozes.FindAsync(id);

        public async Task<IEnumerable<IAnswer>> GetAnswersForDeckAsync(int deckID) =>
            await db.Answers.Where(a => a.DeckID == deckID).ToListAsync();

        public void AddDeck(IDeck deck) =>
            db.Decks.Add(deck as Deck);

        public void AddCard(ICard card) =>
            db.Cards.Add(card as Card);

        public void AddCloze(ICloze cloze) =>
            db.Clozes.Add(cloze as Cloze);

        public void AddRepetition(IUserRepetition repetition) =>
            db.Repetitions.Add(repetition as UserRepetition);

        public void RemoveDeck(IDeck deck) =>
            db.Decks.Remove(deck as Deck);

        public void RemoveCard(ICard card) =>
            db.Cards.Remove(card as Card);

        public void RemoveCloze(ICloze cloze) =>
            db.Clozes.Remove(cloze as Cloze);

        public void RemoveRepetition(IUserRepetition repetition) =>
             db.Repetitions.Remove(repetition as UserRepetition);

        public void AddClozes(ICard card, IEnumerable<string> clozeNames)
        {
            var deck = card.GetDeck();
            var users = card.GetUsers().Concat(deck.Owner).Distinct();

            foreach (var clozeName in clozeNames)
            {
                var newCloze = new Cloze(card.ID, clozeName);

                AddCloze(newCloze);

                foreach (var user in users)
                {
                    var repetition = new UserRepetition
                    {
                        UserName = user,
                        ClozeID = newCloze.ID
                    };

                    var repetitions = deck.GetRepetitions(user);

                    scheduler.PrepareForAdding(deck, repetitions, repetition);
                    AddRepetition(repetition);
                }
            }
        }

        public void RemoveClozes(ICard card, IEnumerable<string> clozeNames)
        {
            foreach (var clozeName in clozeNames)
            {
                var cloze = card.GetClozes().Single(item => item.Label == clozeName);

                db.Clozes.Remove(cloze as Cloze);
            }
        }

        public void AddAnswer(ICloze cloze, bool isCorrect)
        {
            var card = cloze.GetCard();
            var deck = card.GetDeck();

            var answer = new Answer
            {
                Time = DateTime.Now,
                Owner = deck.Owner,
                ClozeID = cloze.ID,
                CardID = card.ID,
                DeckID = deck.ID,
                IsCorrect = isCorrect,
            };

            db.Answers.Add(answer);
        }

        public void PromoteCloze(IDeck deck, Delays delay, string username)
        {
            var repetitions = deck.GetRepetitions(username);

            if (Settings.Default.EnableSiblingsHandling)
            {
                siblingsManager.RearrangeSiblings(deck, repetitions);
            }

            if (Settings.Default.EnableNewCardsHandling)
            {
                newCardsManager.RearrangeNewRepetitions(deck, repetitions);
            }

            scheduler.PromoteRepetition(deck, repetitions, delay);
        }

        public Task SaveChangesAsync() =>
            db.SaveChangesAsync();

        #region IDisposable Support

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    db?.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose() =>
            Dispose(true);

        #endregion

    }
}
