using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface ISchedulerService
    {
        Task PromoteCloze(IDeck deck, Delays delay);
        Task ShuffleNewClozes(int deckID);
    }
}