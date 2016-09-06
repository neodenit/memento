using System.Collections.Generic;

namespace Memento.Interfaces
{
    public interface INewClozesManager
    {
        void RearrangeNewRepetitions(IDeck deck, IEnumerable<IUserRepetition> repetitions);
    }
}