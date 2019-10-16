using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Memento.Common;
using Memento.Interfaces;
using Memento.Models.Helpers;
using Memento.Models.Models;
using static Memento.Core.Scheduler.SchedulerUtils;

namespace Memento.Core.Scheduler
{
    public class Scheduler : IScheduler
    {
        public void PromoteRepetition(Deck deck, IEnumerable<UserRepetition> repetitions, Delays delay)
        {
            var repetition = GetFirstRepetition(repetitions);

            var step = GetStep(deck, delay, repetition.LastDelay);

            var maxRandomPart = (int)Math.Round(step * Settings.Default.RandomizationCoeff);
            var randomPart = GetRandomPart(0, maxRandomPart);
            var correctedStep = step + randomPart;

            var maxNewPosition = GetMaxPosition(repetitions);
            var newPosition = Math.Min(correctedStep, maxNewPosition);
            var newDelay = newPosition > deck.StartDelay || deck.AllowSmallDelays ? newPosition : deck.StartDelay;

            MoveRepetition(repetitions, repetition.Position, newPosition, newDelay, false, true);

            repetition.IsNew = false;

            Debug.Assert(ModelHelpers.ArePositionsValid(repetitions));
        }

        public void PrepareForAdding(Deck deck, IEnumerable<UserRepetition> repetitions, UserRepetition repetition)
        {
            var maxNewPosition = GetMaxNewPosition(repetitions);

            repetition.Position = maxNewPosition;
            repetition.LastDelay = deck.StartDelay;
            repetition.IsNew = true;
        }

        public void PrepareForRemoving(Deck deck, IEnumerable<UserRepetition> repetitions, UserRepetition repetition)
        {
            var position = repetition.Position;
            var movedRepetitions = GetRestRepetitions(repetitions, position);

            DecreasePosition(movedRepetitions);
            DecreaseDelays(movedRepetitions);
        }

        public void ShuffleNewRepetitions(IEnumerable<UserRepetition> repetitions)
        {
            var newRepetitions = from repetition in repetitions where repetition.IsNew select repetition;
            ShuffleRepetitions(newRepetitions);

            Debug.Assert(ModelHelpers.ArePositionsValid(repetitions));
        }

        public void MoveRepetition(IEnumerable<UserRepetition> repetitions, int oldPosition, int newPosition, int newDelay, bool correctMovedRepetitionsDelays, bool correctRestRepetitionsDelays)
        {
            Debug.Assert(newPosition >= oldPosition);

            var movedRepetition = repetitions.Single(repetition => repetition.Position == oldPosition);
            movedRepetition.Position = -1;

            var deck = movedRepetition.GetCloze().GetCard().GetDeck();

            var newLimitedPosition = Math.Min(newPosition, GetMaxPosition(repetitions));

            var minDelay = deck.AllowSmallDelays ? 1 : deck.StartDelay;
            var maxDelay = newLimitedPosition - oldPosition;
            var limitedDelay = Math.Min(newDelay, maxDelay);
            var minLimitedDelay = Math.Max(limitedDelay, minDelay);

            var movedRepetitions = GetRange(repetitions, oldPosition + 1, newLimitedPosition);

            DecreasePosition(movedRepetitions);

            if (correctMovedRepetitionsDelays)
            {
                DecreaseDelays(movedRepetitions);
            }

            if (correctRestRepetitionsDelays)
            {
                var restRepetitions = GetRestRepetitions(repetitions, newLimitedPosition);
                IncreaseDelays(restRepetitions);
            }

            movedRepetition.Position = newLimitedPosition;
            movedRepetition.LastDelay = minLimitedDelay;

            Debug.Assert(ModelHelpers.ArePositionsValid(repetitions));
        }
    }
}
