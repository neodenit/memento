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

        internal static void ShuffleClozes(IEnumerable<ICloze> clozes)
        {
            var positions = from cloze in clozes select cloze.Position;

            var shuffledNumbers = positions.OrderBy(item => Guid.NewGuid());

            var zip = Enumerable.Zip(clozes, shuffledNumbers, (cloze, newPos) => new { cloze, newPos });

            zip.ToList().ForEach(item => item.cloze.Position = item.newPos);
        }

        internal static IEnumerable<ICloze> GetRestClozes(IEnumerable<ICloze> clozes, int position) =>
            from cloze in clozes where cloze.Position >= position select cloze;

        internal static IEnumerable<ICloze> GetRange(IEnumerable<ICloze> clozes, int minPosition, int maxPosition) =>
            from cloze in clozes
            where cloze.Position >= minPosition && cloze.Position <= maxPosition
            select cloze;

        internal static void IncreasePosition(IEnumerable<ICloze> clozes)
        {
            foreach (var cloze in clozes)
            {
                cloze.Position++;
            }
        }

        internal static void DecreasePosition(IEnumerable<ICloze> clozes)
        {
            foreach (var cloze in clozes)
            {
                cloze.Position--;
            }
        }

        internal static void IncreaseDelays(IEnumerable<ICloze> clozes)
        {
            foreach (var cloze in clozes)
            {
                if (!cloze.IsNew)
                {
                    cloze.LastDelay++;
                }
            }
        }

        internal static void DecreaseDelays(IEnumerable<ICloze> clozes)
        {
            foreach (var cloze in clozes)
            {
                if (!cloze.IsNew)
                {
                    cloze.LastDelay--;
                }
            }
        }

        internal static int GetMaxPosition(IEnumerable<ICloze> clozes) =>
            clozes.Any() ? clozes.Max(cloze => cloze.Position) : 0;

        internal static int GetMaxNewPosition(IEnumerable<ICloze> clozes)
        {
            if (clozes.Any())
            {
                var max = clozes.Max(cloze => cloze.Position);
                var nextToMax = max + 1;
                return nextToMax;
            }
            else
            {
                return 0;
            }
        }

        internal static ICloze GetFirstCloze(IEnumerable<ICloze> clozes) =>
            clozes.GetMinElement(cloze => cloze.Position);

        [Obsolete]
        internal static void ReservePosition(IEnumerable<ICloze> clozes, int position, bool correctDelays)
        {
            var movedClozes = from cloze in clozes where cloze.Position >= position select cloze;

            IncreasePosition(movedClozes);

            if (correctDelays)
            {
                IncreaseDelays(movedClozes);
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
