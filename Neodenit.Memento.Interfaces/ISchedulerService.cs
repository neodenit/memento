using System;
using System.Threading.Tasks;
using Neodenit.Memento.Common.Enums;
using Neodenit.Memento.Models.ViewModels;

namespace Neodenit.Memento.Interfaces
{
    public interface ISchedulerService
    {
        Delays GetDelayForWrongAnswer(DelayModes delayMode);

        Task<ViewCardViewModel> PromoteClozeAsync(Guid cardId, Delays delay, string userName);

        Task ShuffleNewClozes(Guid deckId, string username);
    }
}