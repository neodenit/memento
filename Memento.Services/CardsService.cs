﻿using Memento.Additional;
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

        public async Task<IAnswerCardViewModel> EvaluateCard(IAnswerCardViewModel card, string username)
        {
            var dbCard = await FindCardAsync(card.ID);
            var cloze = dbCard.GetNextCloze(username);
            var correctAnswer = converter.GetShortAnswer(dbCard.Text, cloze.Label);
            var fullAnswer = converter.GetFullAnswer(dbCard.Text, cloze.Label);

            var mark = evaluator.Evaluate(correctAnswer, card.UserAnswer);

            var cardWithAnswer = new AnswerCardViewModel(dbCard)
            {
                Mark = mark,
                Question = card.Question,
                FullAnswer = fullAnswer,
                ShortAnswer = correctAnswer,
                UserAnswer = card.UserAnswer,
                Comment = dbCard.Comment,
            };

            return cardWithAnswer;
        }

        public Task<ICard> FindCardAsync(int id) =>
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

        public async Task AddCard(int cardID, int deckID, string text, string comment, string username)
        {
            var clozeNames = converter.GetClozeNames(text);
            var deck = await repository.FindDeckAsync(deckID);

            var newCard = factory.CreateCard(deck, text, comment, true);

            repository.AddCard(newCard);

            await repository.SaveChangesAsync();

            repository.AddClozes(newCard, clozeNames, username);

            await repository.SaveChangesAsync();
        }

        public async Task UpdateCard(int cardID, string text, string comment, string username)
        {
            var dbCard = await FindCardAsync(cardID);
            var clozes = converter.GetClozeNames(dbCard.Text);

            dbCard.Text = text;
            dbCard.Comment = comment;

            var oldClozes = from cloze in dbCard.GetClozes() select cloze.Label;
            var newClozes = clozes;

            var deletedClozes = oldClozes.Except(newClozes).ToList();
            var addedClozes = newClozes.Except(oldClozes).ToList();

            repository.RemoveClozes(dbCard, deletedClozes, username);
            repository.AddClozes(dbCard, addedClozes, username);

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
