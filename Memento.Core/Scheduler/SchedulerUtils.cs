using Memento.Common;
using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        internal static void ShuffleRepetitions(IEnumerable<IUserRepetition> repetitions)
        {
            var positions = from repetition in repetitions select repetition.Position;

            var shuffledNumbers = positions.OrderBy(item => Guid.NewGuid());

            var zip = Enumerable.Zip(repetitions, shuffledNumbers, (repetition, newPos) => new { repetition, newPos });

            zip.ToList().ForEach(item => item.repetition.Position = item.newPos);
        }

        internal static IEnumerable<IUserRepetition> GetRestRepetitions(IEnumerable<IUserRepetition> repetitions, int position) =>
            from repetition in repetitions where repetition.Position >= position select repetition;

        internal static IEnumerable<IUserRepetition> GetRange(IEnumerable<IUserRepetition> repetitions, int minPosition, int maxPosition) =>
            from repetition in repetitions
            where repetition.Position >= minPosition && repetition.Position <= maxPosition
            select repetition;

        internal static void IncreasePosition(IEnumerable<IUserRepetition> repetitions)
        {
            foreach (var repetition in repetitions)
            {
                repetition.Position++;
            }
        }

        internal static void DecreasePosition(IEnumerable<IUserRepetition> repetitions)
        {
            foreach (var repetition in repetitions)
            {
                repetition.Position--;
            }
        }

        internal static void IncreaseDelays(IEnumerable<IUserRepetition> repetitions)
        {
            foreach (var repetition in repetitions)
            {
                if (!repetition.IsNew)
                {
                    repetition.LastDelay++;
                }
            }
        }

        internal static void DecreaseDelays(IEnumerable<IUserRepetition> repetitions)
        {
            foreach (var repetition in repetitions)
            {
                if (!repetition.IsNew)
                {
                    repetition.LastDelay--;
                }
            }
        }

        internal static int GetMaxPosition(IEnumerable<IUserRepetition> repetitions) =>
            repetitions.Any() ? repetitions.Max(repetition => repetition.Position) : 0;

        internal static int GetMaxNewPosition(IEnumerable<IUserRepetition> repetitions)
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

        internal static IUserRepetition GetFirstRepetition(IEnumerable<IUserRepetition> repetitions) =>
            repetitions.GetMinElement(repetition => repetition.Position);

        [Obsolete]
        internal static void ReservePosition(IEnumerable<IUserRepetition> repetitions, int position, bool correctDelays)
        {
            var movedRepetitions = from repetition in repetitions where repetition.Position >= position select repetition;

            IncreasePosition(movedRepetitions);

            if (correctDelays)
            {
                IncreaseDelays(movedRepetitions);
            }
        }

        internal static int GetStep(IDeck deck, Delays delay, int lastDelay)
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
