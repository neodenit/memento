using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Memento.Interfaces;
using Memento.Models.Enums;
using Memento.Models.Models;

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

        public async Task PromoteCloze(Deck deck, Delays delay, string username)
        {
            repository.PromoteCloze(deck, delay, username);

            await repository.SaveChangesAsync();
        }

        public async Task ShuffleNewClozes(Guid deckID, string username)
        {
            var deck = await repository.FindDeckAsync(deckID);

            var clozes = deck.GetRepetitions(username);

            scheduler.ShuffleNewRepetitions(clozes);

            await repository.SaveChangesAsync();
        }
    }
}
