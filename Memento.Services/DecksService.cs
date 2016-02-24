using Memento.Interfaces;
using Memento.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Services
{
    public class DecksService : IDecksService
    {
        private readonly IMementoRepository repository;
        private readonly IConverter converter;
        private readonly IValidator validator;
        private readonly IScheduler scheduler;

        public DecksService(IMementoRepository repository, IConverter converter, IValidator validator, IScheduler scheduler)
        {
            this.repository = repository;
            this.converter = converter;
            this.validator = validator;
            this.scheduler = scheduler;
        }

        public async Task<IEnumerable<IDeck>> GetDecksAsync(string username)
        {
            var decks = await repository.GetUserDecksAsync(username);
            var orderedDecks = decks.OrderBy(deck => deck.Title);
            return orderedDecks;
        }

        public async Task<IEnumerable<IAnswer>> GetAnswersAsync(int deckID, DateTime startTime)
        {
            var answers = await repository.GetAnswersForDeckAsync(deckID);
            return answers.Where(answer => answer.Time >= startTime);
        }
    }
}
