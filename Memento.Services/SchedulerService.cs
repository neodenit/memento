using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Services
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IMementoRepository repository;
        private readonly IScheduler scheduler;

        public SchedulerService(IMementoRepository repository, IScheduler scheduler)
        {
            this.repository = repository;
            this.scheduler = scheduler;
        }

        public async Task PromoteCloze(IDeck deck, Delays delay)
        {
            repository.PromoteCloze(deck, delay);

            await repository.SaveChangesAsync();
        }

        public async Task ShuffleNewClozes(int deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);

            var clozes = deck.GetClozes();

            scheduler.ShuffleNewClozes(clozes);

            await repository.SaveChangesAsync();
        }
    }
}
