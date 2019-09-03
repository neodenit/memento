using System;
using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface ICardsService
    {
        Task<ICard> GetNextCardAsync(int deckID, string username);
        Task<ICard> FindCardAsync(Guid id);

        Task AddAltAnswer(ICloze cloze, string answer);
        IAnswerCardViewModel GetCardWithQuestion(ICloze cloze);
        IAnswerCardViewModel GetCardWithAnswer(ICloze cloze);
        IAnswerCardViewModel EvaluateCard(ICloze cloze, string userAnswer);

        Task AddCard(IEditCardViewModel card);
        Task UpdateCard(IEditCardViewModel card);
        Task DeleteCard(Guid id);
        Task RestoreCard(Guid id);
    }
}