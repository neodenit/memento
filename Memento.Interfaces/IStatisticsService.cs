using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Memento.Additional;
using Memento.Models.Models;

namespace Memento.Interfaces
{
    public interface IStatisticsService
    {
        Task<IEnumerable<Answer>> GetAnswersAsync(int deckID, DateTime startTime);
        Statistics GetStatistics(IEnumerable<Answer> answers);
        Task AddAnswer(Guid cardID, bool isCorrect, string username);
    }
}