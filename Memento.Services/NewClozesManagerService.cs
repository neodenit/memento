using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Memento.Common;
using Memento.Interfaces;
using Memento.Models.Helpers;
using Memento.Models.Models;

namespace Memento.Services
{
    public class NewClozesManagerService : INewClozesManagerService
    {
        private readonly ISchedulerOperationService scheduler;

        public NewClozesManagerService(ISchedulerOperationService scheduler)
        {
            this.scheduler = scheduler;
        }

        public void RearrangeNewRepetitions(Deck deck, IEnumerable<UserRepetition> repetitions)
        {
            var activeRepetitions = repetitions.Take(deck.StartDelay);

            if (!activeRepetitions.Any(repetition => repetition.IsNew) && repetitions.Any(repetition => repetition.IsNew))
            {
                var newRepetition = repetitions.Where(repetition => repetition.IsNew).GetMinElement(repetition => repetition.Position);

                scheduler.MoveRepetition(repetitions, newRepetition.Position, deck.StartDelay, newRepetition.LastDelay, true, false);
            }

            Debug.Assert(ModelHelpers.ArePositionsValid(repetitions));
        }
    }
}
