using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neodenit.Memento.DataAccess.API.DataModels;
using Neodenit.Memento.Models.ViewModels;

namespace Neodenit.Memento.Interfaces
{
    public interface IStatisticsService
    {
        Task<IEnumerable<Answer>> GetAnswersAsync(Guid deckID, DateTime startTime);

        StatisticsViewModel GetStatistics(IEnumerable<Answer> answers);

        Task AddAnswer(Guid cardID, bool isCorrect, string username);
    }
}