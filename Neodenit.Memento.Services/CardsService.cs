using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Neodenit.Memento.DataAccess.API;
using Neodenit.Memento.DataAccess.API.DataModels;
using Neodenit.Memento.Interfaces;
using Neodenit.Memento.Models.ViewModels;

namespace Neodenit.Memento.Services
{
    public class CardsService : ICardsService
    {
        private readonly IMapper mapper;
        private readonly IMementoRepository repository;
        private readonly IConverterService converter;
        private readonly IEvaluatorService evaluator;

        public CardsService(IMapper mapper, IMementoRepository repository, IConverterService converter, IEvaluatorService evaluator)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.converter = converter ?? throw new ArgumentNullException(nameof(converter));
            this.evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        public async Task<ViewCardViewModel> FindCardAsync(Guid id)
        {
            var card = await repository.FindCardAsync(id);
            var viewModel = mapper.Map<ViewCardViewModel>(card);
            return viewModel;
        }

        public async Task<ViewCardViewModel> GetNextCardAsync(Guid deckId, string userName)
        {
            var deck = await repository.FindDeckAsync(deckId);

            if (deck.GetValidCards().Any())
            {
                var card = deck.GetNextCard(userName);
                var viewModel = mapper.Map<ViewCardViewModel>(card);
                return viewModel;
            }
            else
            {
                return null;
            }
        }

        public async Task AddCard(EditCardViewModel card)
        {
            var clozeNames = converter.GetClozeNames(card.Text);
            var deck = await repository.FindDeckAsync(card.DeckID);

            var newCard = new Card
            {
                Deck = deck,
                Text = card.Text,
                Comment = card.Comment,
                IsValid = true,
                ID = card.ID != Guid.Empty ? card.ID : Guid.NewGuid(),
                ReadingCardId = card.ReadingCardId
            };

            repository.AddCard(newCard);

            await repository.SaveChangesAsync();

            await repository.AddClozesAsync(newCard, clozeNames);

            await repository.SaveChangesAsync();
        }

        public async Task UpdateCard(EditCardViewModel card)
        {
            var dbCard = await repository.FindCardAsync(card.ID);
            var clozes = converter.GetClozeNames(dbCard.Text);

            dbCard.Text = card.Text;
            dbCard.Comment = card.Comment;

            var oldClozes = from cloze in dbCard.Clozes select cloze.Label;
            var newClozes = clozes;

            var deletedClozes = oldClozes.Except(newClozes).ToList();
            var addedClozes = newClozes.Except(oldClozes).ToList();

            repository.RemoveClozes(dbCard, deletedClozes);
            await repository.AddClozesAsync(dbCard, addedClozes);

            await repository.SaveChangesAsync();
        }

        public async Task DeleteCard(Guid id)
        {
            var card = await repository.FindCardAsync(id);

            if (card.IsDeleted)
            {
                repository.RemoveCard(card);
            }
            else
            {
                card.IsDeleted = true;
            }

            await repository.SaveChangesAsync();
        }

        public async Task RestoreCard(Guid id)
        {
            var card = await repository.FindCardAsync(id);

            card.IsDeleted = false;

            await repository.SaveChangesAsync();
        }

        public async Task<bool> IsCardValidAsync(Guid readingCardId, Guid repetitionCardId)
        {
            var card = await repository.FindCardAsync(repetitionCardId);
            var isValid = card?.ReadingCardId == readingCardId;
            return isValid;
        }

        public async Task<ClozeViewModel> GetNextClozeAsync(Guid cardId, string userName)
        {
            var card = await repository.FindCardAsync(cardId);
            var cloze = card.GetNextCloze(userName);

            var viewModel = mapper.Map<Cloze, ClozeViewModel>(cloze, opt =>
                                opt.AfterMap((src, dest) =>
                                {
                                    var repetition = cloze.GetUserRepetition(userName);

                                    dest.Position = repetition.Position;
                                    dest.IsNew = repetition.IsNew;
                                    dest.LastDelay = repetition.LastDelay;
                                }));

            return viewModel;
        }

        public async Task<AnswerCardViewModel> GetCardWithAnswerAsync(Guid cardId, string userName)
        {
            var card = await repository.FindCardAsync(cardId);
            var cloze = card.GetNextCloze(userName);
            var fullAnswer = converter.GetFullAnswer(card.Text, cloze.Label);

            var viewModel = mapper.Map<Card, AnswerCardViewModel>(card, opt =>
                                opt.AfterMap((src, dest) =>
                                {
                                    dest.FullAnswer = fullAnswer;
                                }));

            return viewModel;
        }

        public async Task<AnswerCardViewModel> GetCardWithQuestionAsync(Guid cardId, string userName)
        {
            var card = await repository.FindCardAsync(cardId);
            var cloze = card.GetNextCloze(userName);
            var question = converter.GetQuestion(card.Text, cloze.Label);

            var viewModel = mapper.Map<Card, AnswerCardViewModel>(card, opt =>
                                opt.AfterMap((src, dest) =>
                                {
                                    dest.Question = question;
                                }));

            return viewModel;
        }

        public async Task AddAltAnswerAsync(Guid cardId, string altAnswer, string userName)
        {
            var card = await repository.FindCardAsync(cardId);
            var cloze = card.GetNextCloze(userName);

            card.Text = converter.AddAltAnswer(card.Text, cloze.Label, altAnswer);

            await repository.SaveChangesAsync();
        }

        public async Task<AnswerCardViewModel> EvaluateCardAsync(Guid cardId, string userAnswer, string userName)
        {
            var card = await repository.FindCardAsync(cardId);
            var cloze = card.GetNextCloze(userName);

            var question = converter.GetQuestion(card.Text, cloze.Label);
            var fullAnswer = converter.GetFullAnswer(card.Text, cloze.Label);
            var correctAnswer = converter.GetShortAnswer(card.Text, cloze.Label);

            var mark = evaluator.Evaluate(correctAnswer, userAnswer);

            var viewModel = mapper.Map<Card, AnswerCardViewModel>(card, opt =>
                                opt.AfterMap((src, dest) =>
                                {
                                    dest.Mark = mark;
                                    dest.Question = question;
                                    dest.FullAnswer = fullAnswer;
                                    dest.ShortAnswer = correctAnswer;
                                    dest.UserAnswer = userAnswer;
                                }));

            return viewModel;
        }

        public async Task<EditCardViewModel> FindEditCardAsync(Guid id)
        {
            var card = await repository.FindCardAsync(id);
            var viewModel = mapper.Map<EditCardViewModel>(card);
            return viewModel;
        }
    }
}
