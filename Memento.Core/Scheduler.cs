using Memento.Common;
using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Core
{
    public class Scheduler : IScheduler
    {
        private Random random = new Random();

        public void PromoteCloze(IDeck deck, IEnumerable<ICloze> clozes, Delays delay)
        {
            var cloze = GetFirstCloze(clozes);

            var maxNewPosition = GetMaxPosition(clozes);

            int randomPart = GetRandomPart();

            var step = GetStep(deck, delay, cloze.LastDelay);

            var correctedStep = Settings.Default.AddRandomization ? step + randomPart : step;

            var newPosition = Math.Min(correctedStep, maxNewPosition);

            var newDelay = newPosition > deck.StartDelay || deck.AllowSmallDelays ? newPosition : deck.StartDelay;

            MoveCloze(clozes, cloze.Position, newPosition, newDelay, false, true);

            cloze.IsNew = false;
        }

        private int GetRandomPart()
        {
            return random.Next(2); // 0 or 1
        }

        public void AddCloze(IDeck deck, ICollection<ICloze> clozes, ICloze cloze)
        {
            PrepareForAdding(deck, clozes, cloze);

            clozes.Add(cloze);
        }

        public void PrepareForAdding(IDeck deck, IEnumerable<ICloze> clozes, ICloze cloze)
        {
            var maxNewPosition = GetMaxNewPosition(clozes);

            cloze.Position = maxNewPosition;

            cloze.LastDelay = deck.StartDelay;

            cloze.IsNew = true;
        }

        public void PrepareForRemoving(IDeck deck, IEnumerable<ICloze> clozes, ICloze cloze)
        {
            var position = cloze.Position;

            var movedClozes = GetRestClozes(clozes, position);

            DecreasePosition(movedClozes);

            DecreaseDelays(movedClozes);
        }

        public void ShuffleNewClozes(IEnumerable<ICloze> clozes)
        {
            var newClozes = from cloze in clozes where cloze.IsNew select cloze;

            ShuffleClozes(newClozes);
        }

        public void MoveCloze(IEnumerable<ICloze> clozes, int oldPosition, int newPosition, int newDelay, bool correctMovedClozesDelays, bool correctRestClozesDelays)
        {
            var movedCloze = clozes.Single(cloze => cloze.Position == oldPosition);

            movedCloze.Position = -1;

            var newLimitedPosition =
                oldPosition > newPosition ?
                Math.Max(newPosition, 0) :
                Math.Min(newPosition, GetMaxPosition(clozes));

            if (oldPosition > newPosition)
            {
                var movedClozes = GetRange(clozes, newLimitedPosition, oldPosition - 1);

                IncreasePosition(movedClozes);

                if (correctMovedClozesDelays)
                {
                    IncreaseDelays(movedClozes);
                }
            }
            else
            {
                var movedClozes = GetRange(clozes, oldPosition + 1, newLimitedPosition);

                DecreasePosition(movedClozes);

                if (correctMovedClozesDelays)
                {
                    DecreaseDelays(movedClozes);
                }

                if (correctRestClozesDelays)
                {
                    var restClozes = GetRestClozes(clozes, newLimitedPosition);

                    IncreaseDelays(restClozes);
                }
            }

            movedCloze.Position = newLimitedPosition;
            movedCloze.LastDelay = newDelay;
        }

        private static void ShuffleClozes(IEnumerable<ICloze> clozes)
        {
            var positions = from cloze in clozes select cloze.Position;

            var shuffledNumbers = positions.OrderBy(item => Guid.NewGuid());

            var zip = Enumerable.Zip(clozes, shuffledNumbers, (cloze, newPos) => new { cloze, newPos });

            zip.ToList().ForEach(item => item.cloze.Position = item.newPos);
        }

        private static IEnumerable<ICloze> GetRestClozes(IEnumerable<ICloze> clozes, int position)
        {
            var result = from cloze in clozes where cloze.Position >= position select cloze;

            return result;
        }

        private static IEnumerable<ICloze> GetRange(IEnumerable<ICloze> clozes, int minPosition, int maxPosition)
        {
            var result = from cloze in clozes
                         where cloze.Position >= minPosition && cloze.Position <= maxPosition
                         select cloze;

            return result;
        }

        private static void IncreasePosition(IEnumerable<ICloze> clozes)
        {
            foreach (var cloze in clozes)
            {
                cloze.Position++;
            }
        }

        private static void DecreasePosition(IEnumerable<ICloze> clozes)
        {
            foreach (var cloze in clozes)
            {
                cloze.Position--;
            }
        }

        private static void IncreaseDelays(IEnumerable<ICloze> clozes)
        {
            foreach (var cloze in clozes)
            {
                if (!cloze.IsNew)
                {
                    cloze.LastDelay++;
                }
            }
        }

        private static void DecreaseDelays(IEnumerable<ICloze> clozes)
        {
            foreach (var cloze in clozes)
            {
                if (!cloze.IsNew)
                {
                    cloze.LastDelay--;
                }
            }
        }

        private static int GetMaxPosition(IEnumerable<ICloze> clozes) => clozes.Any() ? clozes.Max(cloze => cloze.Position) : 0;

        private static int GetMaxNewPosition(IEnumerable<ICloze> clozes)
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

        private static ICloze GetFirstCloze(IEnumerable<ICloze> clozes) => clozes.GetMinElement(cloze => cloze.Position);

        [Obsolete]
        private static void ReservePosition(IEnumerable<ICloze> clozes, int position, bool correctDelays)
        {
            var movedClozes = from cloze in clozes where cloze.Position >= position select cloze;

            IncreasePosition(movedClozes);

            if (correctDelays)
            {
                IncreaseDelays(movedClozes);
            }
        }

        private static int GetStep(IDeck deck, Delays delay, int lastDelay)
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
