using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neodenit.Memento.Common.DataModels;
using Neodenit.Memento.Common.ViewModels;
using Neodenit.Memento.DataAccess.API;
using Neodenit.Memento.Services.API;

namespace Neodenit.Memento.Services
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

        public async Task<IEnumerable<Answer>> GetAnswersAsync(Guid deckID, DateTime startTime)
        {
            var answers = await repository.GetAnswersForDeckAsync(deckID);
            return answers.Where(answer => answer.Time >= startTime);
        }

        public StatisticsViewModel GetStatistics(IEnumerable<Answer> answers)
        {
            var groupedAnswers = from answer in answers group answer by answer.Time.Date;
            var orderedGroupedAnswers = from answer in groupedAnswers orderby answer.Key select answer;

            var answerLabels = from item in orderedGroupedAnswers select item.Key.ToShortDateString();
            var answerValues = from item in orderedGroupedAnswers select item.Count();

            var correctAnswerValues = from answer in orderedGroupedAnswers select answer.Count(a => a.IsCorrect);
            var incorrectAnswerValues = from answer in orderedGroupedAnswers select answer.Count(a => !a.IsCorrect);

            var statistics = new StatisticsViewModel
            {
                Answers = new AnswerChartViewModel { Labels = answerLabels, Values = answerValues },
                CorrectAnswers = new CorrectAnswerChartViewModel { Labels = answerLabels, Correct = correctAnswerValues, Incorrect = incorrectAnswerValues },
            };

            return statistics;
        }
    }
}
