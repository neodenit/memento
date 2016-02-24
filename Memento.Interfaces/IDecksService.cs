using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface IDecksService
    {
        Task<IEnumerable<IDeck>> GetDecksAsync(string username);
        Task<IEnumerable<IAnswer>> GetAnswersAsync(int deckID, DateTime startTime);
    }
}