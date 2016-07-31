using Memento.Interfaces;
using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface ICardsService
    {
        Task<ICard> GetNextCardAsync(int deckID, string username);
        Task<ICard> FindCardAsync(int id);

        Task AddAltAnswer(ICloze cloze, string answer);
        IAnswerCardViewModel GetCardWithQuestion(ICloze cloze);
        IAnswerCardViewModel GetCardWithAnswer(ICloze cloze);
        IAnswerCardViewModel EvaluateCard(ICloze cloze, string userAnswer);

        Task AddCard(IEditCardViewModel card);
        Task UpdateCard(int cardID, string text, string comment, string username);//todo
        Task DeleteCard(int id);
        Task RestoreCard(int id);
    }
}