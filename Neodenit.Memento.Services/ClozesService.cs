using System;
using System.Collections.Generic;
using System.Linq;
using Neodenit.Memento.Common;
using Neodenit.Memento.Common.DataModels;
using Neodenit.Memento.Common.Enums;
using Neodenit.Memento.Services.API;

namespace Neodenit.Memento.Services
{
    public class ClozesService : IClozesService
    {
        private readonly ISchedulerOperationService schedulerOperationService;
        private readonly ISiblingsManagerService siblingsManagerService;
        private readonly INewClozesManagerService newClozesManagerService;

        public ClozesService(ISchedulerOperationService schedulerOperationService, ISiblingsManagerService siblingsManagerService, INewClozesManagerService newClozesManagerService)
        {
            this.schedulerOperationService = schedulerOperationService ?? throw new ArgumentNullException(nameof(schedulerOperationService));
            this.siblingsManagerService = siblingsManagerService ?? throw new ArgumentNullException(nameof(siblingsManagerService));
            this.newClozesManagerService = newClozesManagerService ?? throw new ArgumentNullException(nameof(newClozesManagerService));
        }

        public void AddClozes(Card card, IEnumerable<string> clozeNames)
        {
            var deck = card.Deck;
            var users = card.GetUsers().Concat(deck.Owner).Distinct();

            foreach (var clozeName in clozeNames)
            {
                var newCloze = new Cloze
                {
                    CardID = card.ID,
                    Label = clozeName,
                    ID = Guid.NewGuid()
                };

                card.Clozes.Add(newCloze);

                foreach (var user in users)
                {
                    var repetition = new UserRepetition
                    {
                        ID = Guid.NewGuid(),
                        UserName = user,
                        ClozeID = newCloze.ID
                    };

                    var repetitions = deck.GetRepetitions(user);

                    schedulerOperationService.PrepareForAdding(deck, repetitions, repetition);

                    newCloze.UserRepetitions.Add(repetition);
                }
            }
        }

        public void PromoteCloze(Deck deck, Delays delay, string username)
        {
            var repetitions = deck.GetRepetitions(username);

            if (Settings.Default.EnableSiblingsHandling)
            {
                siblingsManagerService.RearrangeSiblings(deck, repetitions);
            }

            if (Settings.Default.EnableNewCardsHandling)
            {
                newClozesManagerService.RearrangeNewRepetitions(deck, repetitions);
            }

            schedulerOperationService.PromoteRepetition(deck, repetitions, delay);
        }
    }
}
