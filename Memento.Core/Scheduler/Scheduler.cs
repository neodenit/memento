using Memento.Common;
using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Memento.Core.Scheduler.SchedulerUtils;

namespace Memento.Core.Scheduler
{
    public class Scheduler : IScheduler
    {
        public void PromoteRepetition(IDeck deck, IEnumerable<IUserRepetition> repetitions, Delays delay)
        {
            var repetition = GetFirstRepetition(repetitions);

            var step = GetStep(deck, delay, repetition.LastDelay);
            var randomPart = GetRandomPart();
            var correctedStep = Settings.Default.AddRandomization ? step + randomPart : step;

            var maxNewPosition = GetMaxPosition(repetitions);
            var newPosition = Math.Min(correctedStep, maxNewPosition);
            var newDelay = newPosition > deck.StartDelay || deck.AllowSmallDelays ? newPosition : deck.StartDelay;

            MoveRepetition(repetitions, repetition.Position, newPosition, newDelay, false, true);

            repetition.IsNew = false;

            Debug.Assert(Helpers.ArePositionsValid(repetitions));
        }

        public void PrepareForAdding(IDeck deck, IEnumerable<IUserRepetition> repetitions, IUserRepetition repetition)
        {
            var maxNewPosition = GetMaxNewPosition(repetitions);

            repetition.Position = maxNewPosition;
            repetition.LastDelay = deck.StartDelay;
            repetition.IsNew = true;
        }

        public void PrepareForRemoving(IDeck deck, IEnumerable<IUserRepetition> repetitions, IUserRepetition repetition)
        {
            var position = repetition.Position;
            var movedRepetitions = GetRestRepetitions(repetitions, position);

            DecreasePosition(movedRepetitions);
            DecreaseDelays(movedRepetitions);
        }

        public void ShuffleNewRepetitions(IEnumerable<IUserRepetition> repetitions)
        {
            var newRepetitions = from repetition in repetitions where repetition.IsNew select repetition;
            ShuffleRepetitions(newRepetitions);

            Debug.Assert(Helpers.ArePositionsValid(repetitions));
        }

        public void MoveRepetition(IEnumerable<IUserRepetition> repetitions, int oldPosition, int newPosition, int newDelay, bool correctMovedRepetitionsDelays, bool correctRestRepetitionsDelays)
        {
            var movedRepetition = repetitions.Single(repetition => repetition.Position == oldPosition);
            movedRepetition.Position = -1;

            var newLimitedPosition =
                oldPosition > newPosition ?
                Math.Max(newPosition, 0) :
                Math.Min(newPosition, GetMaxPosition(repetitions));

            if (oldPosition > newPosition)
            {
                var movedRepetitions = GetRange(repetitions, newLimitedPosition, oldPosition - 1);
                IncreasePosition(movedRepetitions);

                if (correctMovedRepetitionsDelays)
                {
                    IncreaseDelays(movedRepetitions);
                }
            }
            else
            {
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
            }

            movedRepetition.Position = newLimitedPosition;
            movedRepetition.LastDelay = newDelay;

            Debug.Assert(Helpers.ArePositionsValid(repetitions));
        }
    }
}
