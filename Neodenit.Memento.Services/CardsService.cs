using System;
using System.Linq;
using System.Threading.Tasks;
using Neodenit.Memento.Interfaces;
using Neodenit.Memento.Models.DataModels;
using Neodenit.Memento.Models.ViewModels;

namespace Neodenit.Memento.Services
{
    public class CardsService : ICardsService
    {
        private readonly IMementoRepository repository;
        private readonly IConverterService converter;
        private readonly IEvaluatorService evaluator;

        public CardsService(IMementoRepository repository, IConverterService converter, IEvaluatorService evaluator)
        {
            this.repository = repository;
            this.converter = converter;
            this.evaluator = evaluator;
        }

        public async Task AddAltAnswer(Cloze cloze, string answer)
        {
            var card = cloze.Card;

            card.Text = converter.AddAltAnswer(card.Text, cloze.Label, answer);

            await repository.SaveChangesAsync();
        }

        public AnswerCardViewModel GetCardWithQuestion(Cloze cloze)
        {
            var card = cloze.Card;
            var question = converter.GetQuestion(card.Text, cloze.Label);

            var result = new AnswerCardViewModel
            {
                ID = card.ID,
                DeckID = card.DeckID,
                DeckTitle = card.Deck.Title,
                DelayMode = card.Deck.DelayMode,
                Question = question
            };

            return result;
        }

        public AnswerCardViewModel GetCardWithAnswer(Cloze cloze)
        {
            var card = cloze.Card;
            var fullAnswer = converter.GetFullAnswer(card.Text, cloze.Label);
            var comment = card.Comment;

            var result = new AnswerCardViewModel
            {
                ID = card.ID,
                DeckID = card.DeckID,
                DeckTitle = card.Deck.Title,
                DelayMode = card.Deck.DelayMode,
                FullAnswer = fullAnswer,
                Comment = comment
            };

            return result;
        }

        public AnswerCardViewModel EvaluateCard(Cloze cloze, string userAnswer)
        {
            var card = cloze.Card;

            var question = converter.GetQuestion(card.Text, cloze.Label);
            var fullAnswer = converter.GetFullAnswer(card.Text, cloze.Label);
            var correctAnswer = converter.GetShortAnswer(card.Text, cloze.Label);

            var mark = evaluator.Evaluate(correctAnswer, userAnswer);

            var cardWithAnswer = new AnswerCardViewModel
            {
                ID = card.ID,
                DeckID = card.DeckID,
                DeckTitle = card.Deck.Title,
                DelayMode = card.Deck.DelayMode,
                Mark = mark,
                Question = question,
                FullAnswer = fullAnswer,
                ShortAnswer = correctAnswer,
                UserAnswer = userAnswer,
                Comment = card.Comment,
            };

            return cardWithAnswer;
        }

        public Task<Card> FindCardAsync(Guid id) =>
            repository.FindCardAsync(id);

        public async Task<Card> GetNextCardAsync(Guid deckID, string username)
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
            var dbCard = await FindCardAsync(card.ID);
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

        public async Task<bool> IsCardValidAsync(Guid readingCardId, Guid repetitionCardId)
        {
            var card = await repository.FindCardAsync(repetitionCardId);
            var isValid = card?.ReadingCardId == readingCardId;
            return isValid;
        }
    }
}
