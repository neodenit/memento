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
        public void PromoteCloze(IDeck deck, IEnumerable<ICloze> clozes, Delays delay, string username)
        {
            var cloze = GetFirstCloze(clozes, username);

            var step = GetStep(deck, delay, cloze.GetUserRepetition(username).LastDelay);
            var randomPart = GetRandomPart();
            var correctedStep = Settings.Default.AddRandomization ? step + randomPart : step;

            var maxNewPosition = GetMaxPosition(clozes, username);
            var newPosition = Math.Min(correctedStep, maxNewPosition);
            var newDelay = newPosition > deck.StartDelay || deck.AllowSmallDelays ? newPosition : deck.StartDelay;

            MoveCloze(clozes, cloze.GetUserRepetition(username).Position, newPosition, newDelay, false, true, username);

            cloze.GetUserRepetition(username).IsNew = false;
        }

        public void AddCloze(IDeck deck, ICollection<ICloze> clozes, ICloze cloze, string username)
        {
            PrepareForAdding(deck, clozes, cloze, username);
            clozes.Add(cloze);
        }

        public void PrepareForAdding(IDeck deck, IEnumerable<ICloze> clozes, ICloze cloze, string username)
        {
            var maxNewPosition = GetMaxNewPosition(clozes, username);
            cloze.GetUserRepetition(username).Position = maxNewPosition;
            cloze.GetUserRepetition(username).LastDelay = deck.StartDelay;
            cloze.GetUserRepetition(username).IsNew = true;
        }

        public void PrepareForRemoving(IDeck deck, IEnumerable<ICloze> clozes, ICloze cloze, string username)
        {
            var position = cloze.GetUserRepetition(username).Position;
            var movedClozes = GetRestClozes(clozes, position, username);

            DecreasePosition(movedClozes, username);
            DecreaseDelays(movedClozes, username);
        }

        public void ShuffleNewClozes(IEnumerable<ICloze> clozes, string username)
        {
            var newClozes = from cloze in clozes where cloze.GetUserRepetition(username).IsNew select cloze;
            ShuffleClozes(newClozes, username);
        }

        public void MoveCloze(IEnumerable<ICloze> clozes, int oldPosition, int newPosition, int newDelay, bool correctMovedClozesDelays, bool correctRestClozesDelays, string username)
        {
            var movedCloze = clozes.Single(cloze => cloze.GetUserRepetition(username).Position == oldPosition);
            movedCloze.GetUserRepetition(username).Position = -1;

            var newLimitedPosition =
                oldPosition > newPosition ?
                Math.Max(newPosition, 0) :
                Math.Min(newPosition, GetMaxPosition(clozes, username));

            if (oldPosition > newPosition)
            {
                var movedClozes = GetRange(clozes, newLimitedPosition, oldPosition - 1, username);
                IncreasePosition(movedClozes, username);

                if (correctMovedClozesDelays)
                {
                    IncreaseDelays(movedClozes, username);
                }
            }
            else
            {
                var movedClozes = GetRange(clozes, oldPosition + 1, newLimitedPosition, username);

                DecreasePosition(movedClozes, username);

                if (correctMovedClozesDelays)
                {
                    DecreaseDelays(movedClozes, username);
                }

                if (correctRestClozesDelays)
                {
                    var restClozes = GetRestClozes(clozes, newLimitedPosition, username);
                    IncreaseDelays(restClozes, username);
                }
            }

            movedCloze.GetUserRepetition(username).Position = newLimitedPosition;
            movedCloze.GetUserRepetition(username).LastDelay = newDelay;
        }
    }
}
