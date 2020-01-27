using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Neodenit.Memento.Common;
using Neodenit.Memento.Common.DataModels;
using Neodenit.Memento.DataAccess;
using Neodenit.Memento.DataAccess.API;

namespace Memento.DataAccess.Repository
{
    public class EFMementoRepository : IMementoRepository
    {
        private readonly MementoContext db;

        public EFMementoRepository(MementoContext mementoContext)
        {
            this.db = mementoContext ?? throw new ArgumentNullException(nameof(mementoContext));
        }

        public async Task<IEnumerable<Deck>> GetAllDecksAsync() =>
            await db.Decks.ToListAsync();

        public async Task<IEnumerable<Deck>> GetSharedDecksAsync() =>
            await db.Decks.Where(d => d.IsShared).ToListAsync();

        public async Task<IEnumerable<Deck>> GetUserDecksAsync(string userName) =>
            await db.Decks.Include(d => d.Cards).Where(d => d.Owner == userName && !d.IsShared).ToListAsync();

        public Deck FindDeck(Guid id) =>
            db.Decks.Find(id);

        public Card FindCard(Guid id) =>
            db.Cards.Include(c => c.Deck).SingleOrDefault(d => d.ID == id);

        public Cloze FindCloze(Guid id) =>
            db.Clozes.Find(id);

        public async Task<Deck> FindDeckAsync(Guid id) =>
            await db.Decks
                .Include(d => d.Cards)
                .ThenInclude(c => c.Clozes)
                .ThenInclude(c => c.UserRepetitions)
                .SingleOrDefaultAsync(d => d.ID == id);

        public async Task<Card> FindCardAsync(Guid id) =>
            await db.Cards
                .Include(c => c.Deck)
                .ThenInclude(d => d.Cards)
                .ThenInclude(c => c.Clozes)
                .ThenInclude(c => c.UserRepetitions)
                .SingleOrDefaultAsync(c => c.ID == id);

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

        public void RemoveClozes(Card card, IEnumerable<string> clozeNames)
        {
            foreach (var clozeName in clozeNames)
            {
                var cloze = card.Clozes.Single(c => c.Label == clozeName);

                db.Clozes.Remove(cloze as Cloze);
            }
        }

        public void AddAnswer(Cloze cloze, bool isCorrect)
        {
            var card = cloze.Card;
            var deck = card.Deck;

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

        public Task SaveChangesAsync() =>
            db.SaveChangesAsync();

        public IEnumerable<Cloze> GetClozes(Deck deck) =>
            deck.ValidCards.SelectMany(card => card.Clozes ?? Enumerable.Empty<Cloze>());

        public IEnumerable<UserRepetition> GetRepetitions(Deck deck, string username)
        {
            var clozes = GetClozes(deck);
            var userRepetitions = from c in clozes select GetUserRepetition(c, username);
            var result = from ur in userRepetitions where ur != null select ur;

            return result;
        }

        public Card GetNextCard(Deck deck, string username) =>
            deck.ValidCards.GetMinElement(vc => GetUserRepetition(GetNextCloze(vc, username), username).Position);

        public Cloze GetNextCloze(Card card, string username) =>
            card.Clozes.GetMinElement(c => GetUserRepetition(c, username).Position);

        public IEnumerable<string> GetUsers(Card card) =>
            card.Clozes.SelectMany(c => GetUsers(c)).Distinct();

        public UserRepetition GetUserRepetition(Cloze cloze, string username) =>
            cloze.UserRepetitions.SingleOrDefault(ur => ur.UserName == username);

        public IEnumerable<string> GetUsers(Cloze cloze) =>
            cloze.UserRepetitions.Select(ur => ur.UserName).Distinct();
    }
}
