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
        public void PromoteCloze(IDeck deck, IEnumerable<ICloze> clozes, Delays delay)
        {
            var cloze = GetFirstCloze(clozes);

            var step = GetStep(deck, delay, cloze.LastDelay);
            var randomPart = GetRandomPart();
            var correctedStep = Settings.Default.AddRandomization ? step + randomPart : step;

            var maxNewPosition = GetMaxPosition(clozes);
            var newPosition = Math.Min(correctedStep, maxNewPosition);
            var newDelay = newPosition > deck.StartDelay || deck.AllowSmallDelays ? newPosition : deck.StartDelay;

            MoveCloze(clozes, cloze.Position, newPosition, newDelay, false, true);

            cloze.IsNew = false;
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
    }
}
