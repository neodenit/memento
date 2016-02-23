using System.Collections.Generic;
using System.Security.Principal;

namespace Memento.Interfaces
{
    public interface ICard
    {
        int DeckID { get; set; }
        int ID { get; set; }
        bool IsDeleted { get; set; }
        bool IsValid { get; set; }
        string Text { get; set; }

        IDeck GetDeck();
        IEnumerable<ICloze> GetClozes();
        void AddCloze(ICloze cloze);
        ICloze GetNextCloze();
        bool IsAuthorized(IPrincipal user);
    }
}