using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Neodenit.Memento.Common;
using Neodenit.Memento.Common.DataModels;
using Neodenit.Memento.Common.Enums;
using Neodenit.Memento.Services.API;

namespace Neodenit.Memento.Services.Scheduler
{
    public class SchedulerOperationService : ISchedulerOperationService
    {
        private readonly ISchedulerUtilsService schedulerUtilsService;

        public SchedulerOperationService(ISchedulerUtilsService schedulerUtilsService)
        {
            this.schedulerUtilsService = schedulerUtilsService ?? throw new ArgumentNullException(nameof(schedulerUtilsService));
        }

        public void PromoteRepetition(Deck deck, IEnumerable<UserRepetition> repetitions, Delays delay)
        {
            var repetition = schedulerUtilsService.GetFirstRepetition(repetitions);

            var step = schedulerUtilsService.GetStep(deck, delay, repetition.LastDelay);

            var maxRandomPart = (int)Math.Round(step * Settings.Default.RandomizationCoeff);
            var randomPart = schedulerUtilsService.GetRandomPart(0, maxRandomPart);
            var newPosition = step + randomPart;

            MoveRepetition(repetitions, repetition.Position, newPosition, newPosition, false, true);

            repetition.IsNew = false;

            Debug.Assert(ModelHelpers.ArePositionsValid(repetitions));
        }

        public void PrepareForAdding(Deck deck, IEnumerable<UserRepetition> repetitions, UserRepetition repetition)
        {
            var maxNewPosition = schedulerUtilsService.GetMaxNewPosition(repetitions);

            repetition.Position = maxNewPosition;
            repetition.LastDelay = deck.StartDelay;
            repetition.IsNew = true;
        }

        public void PrepareForRemoving(Deck deck, IEnumerable<UserRepetition> repetitions, UserRepetition repetition)
        {
            var position = repetition.Position;
            var movedRepetitions = schedulerUtilsService.GetRestRepetitions(repetitions, position);

            schedulerUtilsService.DecreasePosition(movedRepetitions);
            schedulerUtilsService.DecreaseDelays(movedRepetitions);
        }

        public void ShuffleNewRepetitions(IEnumerable<UserRepetition> repetitions)
        {
            var newRepetitions = from repetition in repetitions where repetition.IsNew select repetition;
            schedulerUtilsService.ShuffleRepetitions(newRepetitions);

            Debug.Assert(ModelHelpers.ArePositionsValid(repetitions));
        }

        public void MoveRepetition(IEnumerable<UserRepetition> repetitions, int oldPosition, int newPosition, int newDelay, bool correctMovedRepetitionsDelays, bool correctRestRepetitionsDelays)
        {
            Debug.Assert(newPosition >= oldPosition);

            var newLimitedPosition = Math.Min(newPosition, schedulerUtilsService.GetMaxPosition(repetitions));

            var movedRepetition = repetitions.Single(repetition => repetition.Position == oldPosition);
            movedRepetition.Position = -1;

            var deck = movedRepetition.Cloze.Card.Deck;

            var minDelay = deck.AllowSmallDelays ? 1 : deck.StartDelay;
            var maxDelay = newLimitedPosition - oldPosition;

            var newLimitedDelay =
                minDelay > maxDelay
                    ? minDelay
                    : newDelay < minDelay
                        ? minDelay
                        : newDelay > maxDelay
                            ? maxDelay
                            : newDelay;

            var movedRepetitions = schedulerUtilsService.GetRange(repetitions, oldPosition + 1, newLimitedPosition);

            schedulerUtilsService.DecreasePosition(movedRepetitions);

            if (correctMovedRepetitionsDelays)
            {
                schedulerUtilsService.DecreaseDelays(movedRepetitions);
            }

            if (correctRestRepetitionsDelays)
            {
                var restRepetitions = schedulerUtilsService.GetRestRepetitions(repetitions, newLimitedPosition);
                schedulerUtilsService.IncreaseDelays(restRepetitions);
            }

            movedRepetition.Position = newLimitedPosition;
            movedRepetition.LastDelay = newLimitedDelay;

            Debug.Assert(ModelHelpers.ArePositionsValid(repetitions));
        }
    }
}
