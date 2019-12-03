using System;
using System.Collections.Generic;
using System.Linq;
using Neodenit.Memento.Common;
using Neodenit.Memento.Interfaces;
using Neodenit.Memento.Models.DataModels;

namespace Neodenit.Memento.Services.Scheduler
{
    public class SchedulerUtilsService : ISchedulerUtilsService
    {
        private readonly Random random;

        public SchedulerUtilsService()
        {
            random = new Random();
        }

        public int GetRandomPart(int minStep = 0, int maxStep = 1) =>
            random.Next(minStep, maxStep + 1);

        public void ShuffleRepetitions(IEnumerable<UserRepetition> repetitions)
        {
            var positions = from repetition in repetitions select repetition.Position;

            var shuffledNumbers = positions.OrderBy(item => Guid.NewGuid());

            var zip = Enumerable.Zip(repetitions, shuffledNumbers, (repetition, newPos) => new { repetition, newPos });

            zip.ToList().ForEach(item => item.repetition.Position = item.newPos);
        }

        public IEnumerable<UserRepetition> GetRestRepetitions(IEnumerable<UserRepetition> repetitions, int position) =>
            from repetition in repetitions where repetition.Position >= position select repetition;

        public IEnumerable<UserRepetition> GetRange(IEnumerable<UserRepetition> repetitions, int minPosition, int maxPosition) =>
            from repetition in repetitions
            where repetition.Position >= minPosition && repetition.Position <= maxPosition
            select repetition;

        public void IncreasePosition(IEnumerable<UserRepetition> repetitions)
        {
            foreach (var repetition in repetitions)
            {
                repetition.Position++;
            }
        }

        public void DecreasePosition(IEnumerable<UserRepetition> repetitions)
        {
            foreach (var repetition in repetitions)
            {
                repetition.Position--;
            }
        }

        public void IncreaseDelays(IEnumerable<UserRepetition> repetitions)
        {
            foreach (var repetition in repetitions)
            {
                if (!repetition.IsNew)
                {
                    repetition.LastDelay++;
                }
            }
        }

        public void DecreaseDelays(IEnumerable<UserRepetition> repetitions)
        {
            foreach (var repetition in repetitions)
            {
                if (!repetition.IsNew)
                {
                    repetition.LastDelay--;
                }
            }
        }

        public int GetMaxPosition(IEnumerable<UserRepetition> repetitions) =>
            repetitions.Any() ? repetitions.Max(repetition => repetition.Position) : 0;

        public int GetMaxNewPosition(IEnumerable<UserRepetition> repetitions)
        {
            if (repetitions.Any())
            {
                var max = repetitions.Max(repetition => repetition.Position);
                var nextToMax = max + 1;
                return nextToMax;
            }
            else
            {
                return 0;
            }
        }

        public UserRepetition GetFirstRepetition(IEnumerable<UserRepetition> repetitions) =>
            repetitions.GetMinElement(repetition => repetition.Position);

        [Obsolete]
        public void ReservePosition(IEnumerable<UserRepetition> repetitions, int position, bool correctDelays)
        {
            var movedRepetitions = from repetition in repetitions where repetition.Position >= position select repetition;

            IncreasePosition(movedRepetitions);

            if (correctDelays)
            {
                IncreaseDelays(movedRepetitions);
            }
        }

        public int GetStep(Deck deck, Delays delay, int lastDelay)
        {
            Func<double, int> op = Utils.Round;

            switch (delay)
            {
                case Delays.Initial:
                    return op(deck.StartDelay);
                case Delays.Previous:
                    return op(lastDelay / deck.Coeff);
                case Delays.Same:
                    return lastDelay;
                case Delays.Next:
                    return op(lastDelay * deck.Coeff);
                case Delays.AfterNext:
                    return op(lastDelay * deck.Coeff * deck.Coeff);
                default:
                    return -1;
            }
        }
    }
}
