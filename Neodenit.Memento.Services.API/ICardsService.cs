using System;
using System.Threading.Tasks;
using Neodenit.Memento.Common.ViewModels;

namespace Neodenit.Memento.Services.API
{
    public interface ICardsService
    {
        Task<ViewCardViewModel> GetNextCardAsync(Guid deckId, string username);

        Task<ViewCardViewModel> FindCardAsync(Guid id);

        ViewCardViewModel FindCard(Guid id);

        Task AddAltAnswerAsync(Guid cardId, string altAnswer, string userName);

        Task<AnswerCardViewModel> GetCardWithQuestionAsync(Guid cardId, string userName);

        Task<AnswerCardViewModel> GetCardWithAnswerAsync(Guid cardId, string userName);

        Task<AnswerCardViewModel> EvaluateCardAsync(Guid cardId, string userAnswer, string userName);

        Task AddCard(EditCardViewModel card);

        Task UpdateCard(EditCardViewModel card);

        Task DeleteCard(Guid id);

        Task RestoreCard(Guid id);

        Task<bool> IsCardValidAsync(Guid readingCardId, Guid repetitionCardId);

        Task<ClozeViewModel> GetNextClozeAsync(Guid cardId, string userName);

        Task<EditCardViewModel> FindEditCardAsync(Guid id);
    }
}