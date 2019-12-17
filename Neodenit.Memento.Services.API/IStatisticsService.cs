using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neodenit.Memento.Common.DataModels;
using Neodenit.Memento.Common.ViewModels;

namespace Neodenit.Memento.Services.API
{
    public interface IStatisticsService
    {
        Task<IEnumerable<Answer>> GetAnswersAsync(Guid deckID, DateTime startTime);

        StatisticsViewModel GetStatistics(IEnumerable<Answer> answers);

        Task AddAnswer(Guid cardID, bool isCorrect, string username);
    }
}