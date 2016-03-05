using Memento.Interfaces;
using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface ICardsService
    {
        Task<ICard> GetNextCardAsync(int deckID);
        Task<ICard> FindCardAsync(int id);

        Task AddAltAnswer(int cardID, string answer);
        Task<IAnswerCardViewModel> GetCardWithQuestion(int cardID);
        Task<IAnswerCardViewModel> GetCardWithAnswer(int cardID);
        Task<IAnswerCardViewModel> EvaluateCard(IAnswerCardViewModel card);

        Task AddCard(int cardID, int deckID, string text);
        Task UpdateCard(int cardID, string text);
        Task DeleteCard(int id);
        Task RestoreCard(int id);
    }
}