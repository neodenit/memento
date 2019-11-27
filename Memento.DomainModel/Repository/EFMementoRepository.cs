using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Memento.Common;
using Memento.Interfaces;
using Memento.Models.Models;

namespace Memento.DataAccess.Repository
{
    public class EFMementoRepository : IMementoRepository
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

        public async Task<IEnumerable<Deck>> GetAllDecksAsync() =>
            await db.Decks.ToListAsync();

        public async Task<IEnumerable<Deck>> GetSharedDecksAsync() =>
            await db.Decks.Where(item => item.IsShared).ToListAsync();

        public async Task<IEnumerable<Deck>> GetUserDecksAsync(string userName) =>
            await db.Decks.Where(item => item.Owner == userName && !item.IsShared).ToListAsync();

        public Deck FindDeck(Guid id) =>
            db.Decks.Find(id);

        public Card FindCard(Guid id) =>
            db.Cards.Find(id);

        public Cloze FindCloze(Guid id) =>
            db.Clozes.Find(id);

        public async Task<Deck> FindDeckAsync(Guid id) =>
            await db.Decks.FindAsync(id);

        public async Task<Card> FindCardAsync(Guid id) =>
            await db.Cards.FindAsync(id);

        public async Task<Cloze> FindClozeAsync(Guid id) =>
            await db.Clozes.FindAsync(id);

        public async Task<IEnumerable<Answer>> GetAllAnswersAsync() =>
            await db.Answers.ToListAsync();

        public async Task<IEnumerable<Answer>> GetAnswersForDeckAsync(Guid deckID) =>
            await db.Answers.Where(a => a.DeckID == deckID).ToListAsync();

        public void AddDeck(Deck deck) =>
            db.Decks.Add(deck as Deck);

        public void AddCard(Card card) =>
            db.Cards.Add(card as Card);

        public void AddCloze(Cloze cloze) =>
            db.Clozes.Add(cloze as Cloze);

        public void AddRepetition(UserRepetition repetition) =>
            db.Repetitions.Add(repetition as UserRepetition);

        public void AddAnswer(Answer answer) =>
            db.Answers.Add(answer);

        public void RemoveDeck(Deck deck) =>
            db.Decks.Remove(deck as Deck);

        public void RemoveCard(Card card) =>
            db.Cards.Remove(card as Card);

        public void RemoveCloze(Cloze cloze) =>
            db.Clozes.Remove(cloze as Cloze);

        public void RemoveRepetition(UserRepetition repetition) =>
             db.Repetitions.Remove(repetition as UserRepetition);

        public void RemoveDecks() =>
            db.Decks.RemoveRange(db.Decks);

        public void RemoveAnswers() =>
            db.Answers.RemoveRange(db.Answers);

        public async Task AddClozesAsync(Card card, IEnumerable<string> clozeNames)
        {
            var deck = card.GetDeck();
            var users = card.GetUsers().Concat(deck.Owner).Distinct();

            foreach (var clozeName in clozeNames)
            {
                var newCloze = new Cloze(card.ID, clozeName)
                {
                    ID = Guid.NewGuid()
                };

                card.Clozes.Add(newCloze);

                foreach (var user in users)
                {
                    var repetition = new UserRepetition
                    {
                        ID =  Guid.NewGuid(),
                        UserName = user,
                        ClozeID = newCloze.ID
                    };

                    var repetitions = deck.GetRepetitions(user);

                    scheduler.PrepareForAdding(deck, repetitions, repetition);

                    newCloze.UserRepetitions.Add(repetition);
                }
            }
        }

        public void RemoveClozes(Card card, IEnumerable<string> clozeNames)
        {
            foreach (var clozeName in clozeNames)
            {
                var cloze = card.GetClozes().Single(item => item.Label == clozeName);

                db.Clozes.Remove(cloze as Cloze);
            }
        }

        public void AddAnswer(Cloze cloze, bool isCorrect)
        {
            var card = cloze.GetCard();
            var deck = card.GetDeck();

            var answer = new Answer
            {
                ID = Guid.NewGuid(),
                Time = DateTime.Now,
                Owner = deck.Owner,
                ClozeID = cloze.ID,
                CardID = card.ID,
                DeckID = deck.ID,
                IsCorrect = isCorrect,
            };

            db.Answers.Add(answer);
        }

        public void PromoteCloze(Deck deck, Delays delay, string username)
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
    }
}
