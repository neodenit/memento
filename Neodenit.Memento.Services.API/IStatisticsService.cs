using System;
using System.Threading.Tasks;
using Neodenit.Memento.Common.ViewModels;

namespace Neodenit.Memento.Services.API
{
    public interface IStatisticsService
    {
        Task<StatisticsViewModel> GetStatisticsAsync(Guid deckID, DateTime startTime);

        Task AddAnswer(Guid cardID, bool isCorrect, string username);
    }
}