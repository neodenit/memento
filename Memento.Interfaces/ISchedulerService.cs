using System.Threading.Tasks;
using Memento.Models.Enums;
using Memento.Models.Models;

namespace Memento.Interfaces
{
    public interface ISchedulerService
    {
        Delays GetDelayForWrongAnswer(DelayModes delayMode);
        Task PromoteCloze(Deck deck, Delays delay, string username);
        Task ShuffleNewClozes(int deckID, string username);
    }
}