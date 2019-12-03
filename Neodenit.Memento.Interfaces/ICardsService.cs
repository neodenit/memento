using System;
using System.Threading.Tasks;
using Neodenit.Memento.Models.DataModels;
using Neodenit.Memento.Models.ViewModels;

namespace Neodenit.Memento.Interfaces
{
    public interface ICardsService
    {
        Task<Card> GetNextCardAsync(Guid deckID, string username);

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