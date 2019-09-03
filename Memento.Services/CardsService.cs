using System;
using System.Linq;
using System.Threading.Tasks;
using Memento.Interfaces;
using Memento.Models.ViewModels;

namespace Memento.Services
{
    public class CardsService : ICardsService
    {
        private readonly IMementoRepository repository;
        private readonly IConverter converter;
        private readonly IEvaluator evaluator;
        private readonly IFactory factory;

        public CardsService(IMementoRepository repository, IConverter converter, IEvaluator evaluator, IFactory factory)
        {
            this.repository = repository;
            this.converter = converter;
            this.evaluator = evaluator;
            this.factory = factory;
        }

        public async Task AddAltAnswer(ICloze cloze, string answer)
        {
            var card = cloze.GetCard();

            card.Text = converter.AddAltAnswer(card.Text, cloze.Label, answer);

            await repository.SaveChangesAsync();
        }

        public IAnswerCardViewModel GetCardWithQuestion(ICloze cloze)
        {
            var card = cloze.GetCard();
            var question = converter.GetQuestion(card.Text, cloze.Label);

            var result = new AnswerCardViewModel(card) { Question = question };

            return result;
        }

        public IAnswerCardViewModel GetCardWithAnswer(ICloze cloze)
        {
            var card = cloze.GetCard();
            var fullAnswer = converter.GetFullAnswer(card.Text, cloze.Label);
            var comment = converter.GetComment(card.Text);

            var result = new AnswerCardViewModel(card) { FullAnswer = fullAnswer, Comment = comment };

            return result;
        }

        public IAnswerCardViewModel EvaluateCard(ICloze cloze, string userAnswer)
        {
            var card = cloze.GetCard();

            var question = converter.GetQuestion(card.Text, cloze.Label);
            var fullAnswer = converter.GetFullAnswer(card.Text, cloze.Label);
            var correctAnswer = converter.GetShortAnswer(card.Text, cloze.Label);

            var mark = evaluator.Evaluate(correctAnswer, userAnswer);

            var cardWithAnswer = new AnswerCardViewModel(card)
            {
                Mark = mark,
                Question = question,
                FullAnswer = fullAnswer,
                ShortAnswer = correctAnswer,
                UserAnswer = userAnswer,
                Comment = card.Comment,
            };

            return cardWithAnswer;
        }

        public Task<ICard> FindCardAsync(Guid id) =>
            repository.FindCardAsync(id);

        public async Task<ICard> GetNextCardAsync(int deckID, string username)
        {
            var dbDeck = await repository.FindDeckAsync(deckID);

            if (dbDeck.GetValidCards().Any())
            {
                return dbDeck.GetNextCard(username);
            }
            else
            {
                return null;
            }
        }

        public async Task AddCard(IEditCardViewModel card)
        {
            var clozeNames = converter.GetClozeNames(card.Text);
            var deck = await repository.FindDeckAsync(card.DeckID);

            var newCard = factory.CreateCard(deck, card.Text, card.Comment, true);

            newCard.ID = Guid.NewGuid();

            repository.AddCard(newCard);

            await repository.SaveChangesAsync();

            await repository.AddClozesAsync(newCard, clozeNames);

            await repository.SaveChangesAsync();
        }

        public async Task UpdateCard(IEditCardViewModel card)
        {
            var dbCard = await FindCardAsync(card.ID);
            var clozes = converter.GetClozeNames(dbCard.Text);

            dbCard.Text = card.Text;
            dbCard.Comment = card.Comment;

            var oldClozes = from cloze in dbCard.GetClozes() select cloze.Label;
            var newClozes = clozes;

            var deletedClozes = oldClozes.Except(newClozes).ToList();
            var addedClozes = newClozes.Except(oldClozes).ToList();

            repository.RemoveClozes(dbCard, deletedClozes);
            await repository.AddClozesAsync(dbCard, addedClozes);

            await repository.SaveChangesAsync();
        }

        public async Task DeleteCard(Guid id)
        {
            var card = await FindCardAsync(id);

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
            var card = await FindCardAsync(id);

            card.IsDeleted = false;

            await repository.SaveChangesAsync();
        }
    }
}
