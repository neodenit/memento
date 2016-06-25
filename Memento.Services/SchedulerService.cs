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

        private readonly Dictionary<DelayModes, Delays> delayMap;

        public SchedulerService(IMementoRepository repository, IScheduler scheduler)
        {
            this.repository = repository;
            this.scheduler = scheduler;

            delayMap = new Dictionary<DelayModes, Delays>
            {
                { DelayModes.Sharp, Delays.Initial },
                { DelayModes.Smooth, Delays.Previous },
            };
        }

        public Delays GetDelayForWrongAnswer(DelayModes delayMode) =>
            delayMap[delayMode];

        public async Task PromoteCloze(IDeck deck, Delays delay, string username)
        {
            repository.PromoteCloze(deck, delay, username);

            await repository.SaveChangesAsync();
        }

        public async Task ShuffleNewClozes(int deckID, string username)
        {
            var deck = await repository.FindDeckAsync(deckID);

            var clozes = deck.GetClozes();

            scheduler.ShuffleNewClozes(clozes, username);

            await repository.SaveChangesAsync();
        }
    }
}
