using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neodenit.Memento.Additional;
using Neodenit.Memento.Models.DataModels;

namespace Neodenit.Memento.Interfaces
{
    public interface IStatisticsService
    {
        Task<IEnumerable<Answer>> GetAnswersAsync(Guid deckID, DateTime startTime);

        Statistics GetStatistics(IEnumerable<Answer> answers);

        Task AddAnswer(Guid cardID, bool isCorrect, string username);
    }
}