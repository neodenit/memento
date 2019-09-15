using System;
using System.Threading.Tasks;
using Memento.Models.Models;
using Memento.Models.ViewModels;

namespace Memento.Interfaces
{
    public interface ICardsService
    {
        Task<Card> GetNextCardAsync(int deckID, string username);
        Task<Card> FindCardAsync(Guid id);

        Task AddAltAnswer(Cloze cloze, string answer);
        AnswerCardViewModel GetCardWithQuestion(Cloze cloze);
        AnswerCardViewModel GetCardWithAnswer(Cloze cloze);
        AnswerCardViewModel EvaluateCard(Cloze cloze, string userAnswer);

        Task AddCard(EditCardViewModel card);
        Task UpdateCard(EditCardViewModel card);
        Task DeleteCard(Guid id);
        Task RestoreCard(Guid id);

        Task<bool> IsCardValidAsync(Guid readingCardId, Guid repetitionCardId);
    }
}