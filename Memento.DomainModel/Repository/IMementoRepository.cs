using Memento.DomainModel.Models;
using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Memento.DomainModel.Repository
{
    public interface IMementoRepository
    {
        IQueryable<Deck> GetUserDecks(string userName);
        IQueryable<Answer> GetAnswersForDeck(int deckID);
        Deck FindDeck(int? id);
        Card FindCard(int? id);
        Cloze FindCloze(int? id);
        Task<Deck> FindDeckAsync(int? id);
        Task<Card> FindCardAsync(int? id);
        Task<Cloze> FindClozeAsync(int? id);
        void AddDeck(Deck deck);
        void AddCard(Card card);
        void AddCloze(Cloze cloze);
        void RemoveDeck(Deck deck);
        void RemoveCard(Card card);
        void RemoveCloze(Cloze cloze);
        void AddClozes(Card card, IEnumerable<string> clozeNames);
        void RemoveClozes(Card card, IEnumerable<string> clozeNames);
        void AddAnswer(Cloze cloze, bool isCorrect);
        void PromoteCard(Deck deck, Delays delay);
        Task SaveChangesAsync();
        void Dispose();
    }
}
