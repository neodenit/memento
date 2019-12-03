using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neodenit.Memento.Common;
using Neodenit.Memento.Models.DataModels;

namespace Neodenit.Memento.Interfaces
{
    public interface IMementoRepository
    {
        Task<IEnumerable<Deck>> GetAllDecksAsync();

        Task<IEnumerable<Deck>> GetSharedDecksAsync();

        Task<IEnumerable<Deck>> GetUserDecksAsync(string userName);

        Task<IEnumerable<Answer>> GetAllAnswersAsync();

        Task<IEnumerable<Answer>> GetAnswersForDeckAsync(Guid deckID);

        Deck FindDeck(Guid id);

        Card FindCard(Guid id);

        Cloze FindCloze(Guid id);

        Task<Deck> FindDeckAsync(Guid id);

        Task<Card> FindCardAsync(Guid id);

        Task<Cloze> FindClozeAsync(Guid id);

        void AddDeck(Deck deck);

        void AddCard(Card card);

        void AddCloze(Cloze cloze);

        void RemoveDeck(Deck deck);

        void RemoveCard(Card card);

        void RemoveCloze(Cloze cloze);

        void RemoveDecks();

        void RemoveAnswers();

        Task AddClozesAsync(Card card, IEnumerable<string> clozeNames);

        void RemoveClozes(Card card, IEnumerable<string> clozeNames);

        void AddAnswer(Answer answer);

        void AddAnswer(Cloze cloze, bool isCorrect);

        void PromoteCloze(Deck deck, Delays delay, string username);

        Task SaveChangesAsync();
    }
}
