using Memento.Additional;
using Memento.Common;
using Memento.Interfaces;
using Memento.Models.Models;
using Memento.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Services
{
    public class CardsService : ICardsService
    {
        private readonly IMementoRepository repository;
        private readonly IConverter converter;
        private readonly IEvaluator evaluator;

        public CardsService(IMementoRepository repository, IConverter converter, IEvaluator evaluator)
        {
            this.repository = repository;
            this.converter = converter;
            this.evaluator = evaluator;
        }

        public async Task AddAltAnswer(int cardID, string answer)
        {
            var dbCard = await FindCardAsync(cardID);
            var cloze = dbCard.GetNextCloze();

            dbCard.Text = converter.AddAltAnswer(dbCard.Text, cloze.Label, answer);

            await repository.SaveChangesAsync();
        }

        public async Task<IAnswerCardViewModel> GetCardWithQuestion(int cardID)
        {
            var card = await FindCardAsync(cardID);
            var cloze = card.GetNextCloze();
            var question = converter.GetQuestion(card.Text, cloze.Label);

            var result = new AnswerCardViewModel(card) { Question = question };

            return result;
        }

        public async Task<IAnswerCardViewModel> GetCardWithAnswer(int cardID)
        {
            var card = await FindCardAsync(cardID);
            var cloze = card.GetNextCloze();
            var answer = converter.GetAnswer(card.Text, cloze.Label);

            var result = new AnswerCardViewModel(card) { Answer = answer };

            return result;
        }

        public async Task<IAnswerCardViewModel> EvaluateCard(IAnswerCardViewModel card)
        {
            var cardWithAnswer = await GetCardWithAnswer(card.ID);

            cardWithAnswer.Mark = evaluator.Evaluate(cardWithAnswer.Text, card.Answer);

            return cardWithAnswer;
        }

        public Task<ICard> FindCardAsync(int id) =>
            repository.FindCardAsync(id);

        public async Task<ICard> GetNextCardAsync(int deckID)
        {
            var dbDeck = await repository.FindDeckAsync(deckID);

            if (dbDeck.GetValidCards().Any())
            {
                return dbDeck.GetNextCard();
            }
            else
            {
                return null;
            }
        }

        public async Task AddCard(int cardID, int deckID, string text)
        {
            var clozeNames = converter.GetClozeNames(text);

            var newCard = new Card
            {
                ID = cardID,
                DeckID = deckID,
                Deck = await repository.FindDeckAsync(deckID) as Deck,
                Text = converter.ReplaceTextWithWildcards(text, clozeNames),
                Clozes = new List<Cloze>(),
                IsValid = true,
                IsDeleted = false,
            };

            repository.AddCard(newCard);

            await repository.SaveChangesAsync();

            repository.AddClozes(newCard, clozeNames);

            await repository.SaveChangesAsync();
        }

        public async Task UpdateCard(int cardID, string text)
        {
            var dbCard = await FindCardAsync(cardID);
            var clozes = converter.GetClozeNames(dbCard.Text);

            dbCard.Text = converter.ReplaceTextWithWildcards(text, clozes);

            var oldClozes = from cloze in dbCard.GetClozes() select cloze.Label;
            var newClozes = clozes;

            var deletedClozes = oldClozes.Except(newClozes).ToList();
            var addedClozes = newClozes.Except(oldClozes).ToList();

            repository.RemoveClozes(dbCard, deletedClozes);
            repository.AddClozes(dbCard, addedClozes);

            await repository.SaveChangesAsync();
        }

        public async Task DeleteCard(int id)
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

        public async Task RestoreCard(int id)
        {
            var card = await FindCardAsync(id);

            card.IsDeleted = false;

            await repository.SaveChangesAsync();
        }
    }
}
