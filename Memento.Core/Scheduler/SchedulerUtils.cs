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

        internal static void ShuffleClozes(IEnumerable<ICloze> clozes, string username)
        {
            var positions = from cloze in clozes select cloze.GetUserRepetition(username).Position;

            var shuffledNumbers = positions.OrderBy(item => Guid.NewGuid());

            var zip = Enumerable.Zip(clozes, shuffledNumbers, (cloze, newPos) => new { cloze, newPos });

            zip.ToList().ForEach(item => item.cloze.GetUserRepetition(username).Position = item.newPos);
        }

        internal static IEnumerable<ICloze> GetRestClozes(IEnumerable<ICloze> clozes, int position, string username) =>
            from cloze in clozes where cloze.GetUserRepetition(username).Position >= position select cloze;

        internal static IEnumerable<ICloze> GetRange(IEnumerable<ICloze> clozes, int minPosition, int maxPosition, string username) =>
            from cloze in clozes
            where cloze.GetUserRepetition(username).Position >= minPosition && cloze.GetUserRepetition(username).Position <= maxPosition
            select cloze;

        internal static void IncreasePosition(IEnumerable<ICloze> clozes, string username)
        {
            foreach (var cloze in clozes)
            {
                cloze.GetUserRepetition(username).Position++;
            }
        }

        internal static void DecreasePosition(IEnumerable<ICloze> clozes, string username)
        {
            foreach (var cloze in clozes)
            {
                cloze.GetUserRepetition(username).Position--;
            }
        }

        internal static void IncreaseDelays(IEnumerable<ICloze> clozes, string username)
        {
            foreach (var cloze in clozes)
            {
                if (!cloze.GetUserRepetition(username).IsNew)
                {
                    cloze.GetUserRepetition(username).LastDelay++;
                }
            }
        }

        internal static void DecreaseDelays(IEnumerable<ICloze> clozes, string username)
        {
            foreach (var cloze in clozes)
            {
                if (!cloze.GetUserRepetition(username).IsNew)
                {
                    cloze.GetUserRepetition(username).LastDelay--;
                }
            }
        }

        internal static int GetMaxPosition(IEnumerable<ICloze> clozes, string username) =>
            clozes.Any() ? clozes.Max(cloze => cloze.GetUserRepetition(username).Position) : 0;

        internal static int GetMaxNewPosition(IEnumerable<ICloze> clozes, string username)
        {
            if (clozes.Any())
            {
                var max = clozes.Max(cloze => cloze.GetUserRepetition(username).Position);
                var nextToMax = max + 1;
                return nextToMax;
            }
            else
            {
                return 0;
            }
        }

        internal static ICloze GetFirstCloze(IEnumerable<ICloze> clozes, string username) =>
            clozes.GetMinElement(cloze => cloze.GetUserRepetition(username).Position);

        [Obsolete]
        internal static void ReservePosition(IEnumerable<ICloze> clozes, int position, bool correctDelays, string username)
        {
            var movedClozes = from cloze in clozes where cloze.GetUserRepetition(username).Position >= position select cloze;

            IncreasePosition(movedClozes, username);

            if (correctDelays)
            {
                IncreaseDelays(movedClozes, username);
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
