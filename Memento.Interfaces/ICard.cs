using System;
using System.Collections.Generic;

namespace Memento.Interfaces
{
    public interface ICard
    {
        int DeckID { get; set; }
        Guid ID { get; set; }
        Guid ReadingCardId { get; set; }
        bool IsDeleted { get; set; }
        bool IsValid { get; set; }
        string Text { get; set; }
        string Comment { get; set; }

        IDeck GetDeck();
        IEnumerable<ICloze> GetClozes();
        void AddCloze(ICloze cloze);
        ICloze GetNextCloze(string username);
        IEnumerable<string> GetUsers();
    }
}