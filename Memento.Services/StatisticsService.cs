using Memento.Additional;
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

        public async Task AddAnswer(Guid cardID, bool isCorrect, string username)
        {
            var dbCard = await repository.FindCardAsync(cardID);
            var cloze = dbCard.GetNextCloze(username);

            repository.AddAnswer(cloze, isCorrect);
            await repository.SaveChangesAsync();
        }

        public async Task<IEnumerable<Answer>> GetAnswersAsync(int deckID, DateTime startTime)
        {
            var answers = await repository.GetAnswersForDeckAsync(deckID);
            return answers.Where(answer => answer.Time >= startTime);
        }

        public Statistics GetStatistics(IEnumerable<Answer> answers)
        {
            var groupedAnswers = from answer in answers group answer by answer.Time.Date;

            var answerLabels = from item in groupedAnswers select item.Key.ToShortDateString();
            var answerValues = from item in groupedAnswers select item.Count();

            var groupedCorrectAnswers = from answer in answers where answer.IsCorrect group answer by answer.Time.Date;

            var correctAnswerLabels = from item in groupedCorrectAnswers select item.Key.ToShortDateString();
            var correctAnswerValues = from item in groupedCorrectAnswers select item.Count();

            var statistics = new Statistics
            {
                Answers = new ChartData(answerLabels, answerValues),
                CorrectAnswers = new ChartData(correctAnswerLabels, correctAnswerValues),
            };

            return statistics;
        }
    }
}
