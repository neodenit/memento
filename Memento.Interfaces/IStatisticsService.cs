using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface IStatisticsService
    {
        Task<IEnumerable<IAnswer>> GetAnswersAsync(int deckID, DateTime startTime);
        IStatistics GetStatistics(IEnumerable<IAnswer> answers);
        Task AddAnswer(int cardID, bool isCorrect, string username);
    }
}