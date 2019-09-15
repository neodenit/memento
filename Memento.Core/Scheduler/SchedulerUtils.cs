using System;
using System.Collections.Generic;
using System.Linq;
using Memento.Common;
using Memento.Interfaces;
using Memento.Models.Models;

namespace Memento.Core.Scheduler
{
    internal static class SchedulerUtils
    {
        private readonly static Random random;

        static SchedulerUtils()
        {
            random = new Random();
        }
        
        internal static int GetRandomPart(int minStep = 0, int maxStep = 1) =>
            random.Next(minStep, maxStep + 1);

        internal static void ShuffleRepetitions(IEnumerable<UserRepetition> repetitions)
        {
            var positions = from repetition in repetitions select repetition.Position;

            var shuffledNumbers = positions.OrderBy(item => Guid.NewGuid());

            var zip = Enumerable.Zip(repetitions, shuffledNumbers, (repetition, newPos) => new { repetition, newPos });

            zip.ToList().ForEach(item => item.repetition.Position = item.newPos);
        }

        internal static IEnumerable<UserRepetition> GetRestRepetitions(IEnumerable<UserRepetition> repetitions, int position) =>
            from repetition in repetitions where repetition.Position >= position select repetition;

        internal static IEnumerable<UserRepetition> GetRange(IEnumerable<UserRepetition> repetitions, int minPosition, int maxPosition) =>
            from repetition in repetitions
            where repetition.Position >= minPosition && repetition.Position <= maxPosition
            select repetition;

        internal static void IncreasePosition(IEnumerable<UserRepetition> repetitions)
        {
            foreach (var repetition in repetitions)
            {
                repetition.Position++;
            }
        }

        internal static void DecreasePosition(IEnumerable<UserRepetition> repetitions)
        {
            foreach (var repetition in repetitions)
            {
                repetition.Position--;
            }
        }

        internal static void IncreaseDelays(IEnumerable<UserRepetition> repetitions)
        {
            foreach (var repetition in repetitions)
            {
                if (!repetition.IsNew)
                {
                    repetition.LastDelay++;
                }
            }
        }

        internal static void DecreaseDelays(IEnumerable<UserRepetition> repetitions)
        {
            foreach (var repetition in repetitions)
            {
                if (!repetition.IsNew)
                {
                    repetition.LastDelay--;
                }
            }
        }

        internal static int GetMaxPosition(IEnumerable<UserRepetition> repetitions) =>
            repetitions.Any() ? repetitions.Max(repetition => repetition.Position) : 0;

        internal static int GetMaxNewPosition(IEnumerable<UserRepetition> repetitions)
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

        internal static UserRepetition GetFirstRepetition(IEnumerable<UserRepetition> repetitions) =>
            repetitions.GetMinElement(repetition => repetition.Position);

        [Obsolete]
        internal static void ReservePosition(IEnumerable<UserRepetition> repetitions, int position, bool correctDelays)
        {
            var movedRepetitions = from repetition in repetitions where repetition.Position >= position select repetition;

            IncreasePosition(movedRepetitions);

            if (correctDelays)
            {
                IncreaseDelays(movedRepetitions);
            }
        }

        internal static int GetStep(Deck deck, Delays delay, int lastDelay)
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
