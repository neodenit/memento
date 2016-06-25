using Memento.Interfaces;
using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface ICardsService
    {
        Task<ICard> GetNextCardAsync(int deckID, string username);
        Task<ICard> FindCardAsync(int id);

        Task AddAltAnswer(int cardID, string answer, string username);//todo
        Task<IAnswerCardViewModel> GetCardWithQuestion(int cardID, string username);//todo
        Task<IAnswerCardViewModel> GetCardWithAnswer(int cardID, string username);//todo
        Task<IAnswerCardViewModel> EvaluateCard(IAnswerCardViewModel card, string username);//todo

        Task AddCard(int cardID, int deckID, string text, string comment, string username);//todo
        Task UpdateCard(int cardID, string text, string comment, string username);//todo
        Task DeleteCard(int id);
        Task RestoreCard(int id);
    }
}