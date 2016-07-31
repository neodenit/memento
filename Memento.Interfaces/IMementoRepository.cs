using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface IMementoRepository
    {
        Task<IEnumerable<IDeck>> GetSharedDecksAsync();
        Task<IEnumerable<IDeck>> GetUserDecksAsync(string userName);
        Task<IEnumerable<IAnswer>> GetAnswersForDeckAsync(int deckID);
        IDeck FindDeck(int id);
        ICard FindCard(int id);
        ICloze FindCloze(int id);
        Task<IDeck> FindDeckAsync(int id);
        Task<ICard> FindCardAsync(int id);
        Task<ICloze> FindClozeAsync(int id);
        void AddDeck(IDeck deck);
        void AddCard(ICard card);
        void AddCloze(ICloze cloze);
        void RemoveDeck(IDeck deck);
        void RemoveCard(ICard card);
        void RemoveCloze(ICloze cloze);
        void AddClozes(ICard card, IEnumerable<string> clozeNames);
        void RemoveClozes(ICard card, IEnumerable<string> clozeNames);
        void AddAnswer(ICloze cloze, bool isCorrect);
        void PromoteCloze(IDeck deck, Delays delay, string username);
        Task SaveChangesAsync();
    }
}
