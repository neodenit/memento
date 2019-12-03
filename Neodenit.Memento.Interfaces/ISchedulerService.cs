using System;
using System.Threading.Tasks;
using Neodenit.Memento.Common;
using Neodenit.Memento.Models.DataModels;
using Neodenit.Memento.Models.Enums;

namespace Neodenit.Memento.Interfaces
{
    public interface ISchedulerService
    {
        Delays GetDelayForWrongAnswer(DelayModes delayMode);

        Task PromoteCloze(Deck deck, Delays delay, string username);

        Task ShuffleNewClozes(Guid deckID, string username);
    }
}