using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface ISchedulerService
    {
        Delays GetDelayForWrongAnswer(DelayModes delayMode);
        Task PromoteCloze(IDeck deck, Delays delay, string username);
        Task ShuffleNewClozes(int deckID, string username);
    }
}