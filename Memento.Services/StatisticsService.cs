﻿using Memento.Additional;
using Memento.Common;
using Memento.Interfaces;
using Memento.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IMementoRepository repository;

        public StatisticsService(IMementoRepository repository)
        {
            this.repository = repository;
        }

        public async Task AddAnswer(int cardID, bool isCorrect)
        {
            var dbCard = await repository.FindCardAsync(cardID);
            var cloze = dbCard.GetNextCloze();

            repository.AddAnswer(cloze, isCorrect);
            await repository.SaveChangesAsync();
        }

        public async Task<IEnumerable<IAnswer>> GetAnswersAsync(int deckID, DateTime startTime)
        {
            var answers = await repository.GetAnswersForDeckAsync(deckID);
            return answers.Where(answer => answer.Time >= startTime);
        }

        public IStatistics GetStatistics(IEnumerable<IAnswer> answers)
        {
            var groupedAnswers = from answer in answers group answer by answer.Time.Date;

            var answerLabels = from item in groupedAnswers select item.Key.ToShortDateString();
            var answerValues = from item in groupedAnswers select item.Count();

            var groupedCorrectAnswers = from answer in answers where answer.IsCorrect group answer by answer.Time.Date;

            var correctAnswerLabels = from item in groupedCorrectAnswers select item.Key.ToShortDateString();
            var correctAnswerValues = from item in groupedCorrectAnswers select item.Count();

            var cardsLabels = answerLabels;
            var cardsValues = from item in groupedAnswers select item.GetMaxElement(x => x.Time).CardsInRepetition;

            var statistics = new Statistics
            {
                Answers = new ChartData(answerLabels, answerValues),
                CorrectAnswers = new ChartData(correctAnswerLabels, correctAnswerValues),
                Cards = new ChartData(cardsLabels, cardsValues),
            };

            return statistics;
        }
    }
}