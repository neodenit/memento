using System.Collections.Generic;

namespace Memento.Interfaces
{
    public interface INewClozesManager
    {
        void RearrangeNewClozes(IDeck deck, IEnumerable<ICloze> clozes, string username);
    }
}