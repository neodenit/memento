using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Memento.Models.Models;

namespace Memento.Interfaces
{
    public interface IMementoRepository
    {
        Task<IEnumerable<Deck>> GetSharedDecksAsync();
        Task<IEnumerable<Deck>> GetUserDecksAsync(string userName);
        Task<IEnumerable<Answer>> GetAnswersForDeckAsync(Guid deckID);
        Deck FindDeck(Guid id);
        Card FindCard(Guid id);
        Cloze FindCloze(int id);
        Task<Deck> FindDeckAsync(Guid id);
        Task<Card> FindCardAsync(Guid id);
        Task<Cloze> FindClozeAsync(int id);
        void AddDeck(Deck deck);
        void AddCard(Card card);
        void AddCloze(Cloze cloze);
        void RemoveDeck(Deck deck);
        void RemoveCard(Card card);
        void RemoveCloze(Cloze cloze);
        Task AddClozesAsync(Card card, IEnumerable<string> clozeNames);
        void RemoveClozes(Card card, IEnumerable<string> clozeNames);
        void AddAnswer(Cloze cloze, bool isCorrect);
        void PromoteCloze(Deck deck, Delays delay, string username);
        Task SaveChangesAsync();
    }
}
