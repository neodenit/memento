using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Neodenit.Memento.Common.Enums;
using Neodenit.Memento.DataAccess.API;
using Neodenit.Memento.Interfaces;
using Neodenit.Memento.Models.ViewModels;

namespace Neodenit.Memento.Services
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IMapper mapper;
        private readonly IMementoRepository repository;
        private readonly ISchedulerOperationService scheduler;

        private readonly Dictionary<DelayModes, Delays> delayMap;

        public SchedulerService(IMapper mapper, IMementoRepository repository, ISchedulerOperationService scheduler)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

            delayMap = new Dictionary<DelayModes, Delays>
            {
                { DelayModes.Sharp, Delays.Initial },
                { DelayModes.Smooth, Delays.Previous },
            };
        }

        public Delays GetDelayForWrongAnswer(DelayModes delayMode) =>
            delayMap[delayMode];

        public async Task<ViewCardViewModel> PromoteClozeAsync(Guid cardId, Delays delay, string userName)
        {
            var dbCard = await repository.FindCardAsync(cardId);
            var deck = dbCard.Deck;

            repository.PromoteCloze(deck, delay, userName);

            await repository.SaveChangesAsync();

            var nextCard = deck.GetNextCard(userName);
            var viewModel = mapper.Map<ViewCardViewModel>(nextCard);
            return viewModel;
        }

        public async Task ShuffleNewClozes(Guid deckId, string username)
        {
            var deck = await repository.FindDeckAsync(deckId);

            var clozes = deck.GetRepetitions(username);

            scheduler.ShuffleNewRepetitions(clozes);

            await repository.SaveChangesAsync();
        }
    }
}
