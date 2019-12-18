using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Neodenit.Memento.Common.Enums;
using Neodenit.Memento.Common.ViewModels;
using Neodenit.Memento.DataAccess.API;
using Neodenit.Memento.Services.API;

namespace Neodenit.Memento.Services
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IMapper mapper;
        private readonly IMementoRepository repository;
        private readonly ISchedulerOperationService schedulerOperationService;
        private readonly IClozesService clozesService;
        private readonly Dictionary<DelayModes, Delays> delayMap;

        public SchedulerService(IMapper mapper, IMementoRepository repository, ISchedulerOperationService schedulerOperationService, IClozesService clozesService)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.schedulerOperationService = schedulerOperationService ?? throw new ArgumentNullException(nameof(schedulerOperationService));
            this.clozesService = clozesService ?? throw new ArgumentNullException(nameof(clozesService));
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

            clozesService.PromoteCloze(deck, delay, userName);

            await repository.SaveChangesAsync();

            var nextCard = deck.GetNextCard(userName);
            var viewModel = mapper.Map<ViewCardViewModel>(nextCard);
            return viewModel;
        }

        public async Task ShuffleNewClozes(Guid deckId, string username)
        {
            var deck = await repository.FindDeckAsync(deckId);

            var clozes = deck.GetRepetitions(username);

            schedulerOperationService.ShuffleNewRepetitions(clozes);

            await repository.SaveChangesAsync();
        }
    }
}
