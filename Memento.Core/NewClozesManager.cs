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
    public class NewClozesManager : INewClozesManager
    {
        private readonly IScheduler scheduler;

        public NewClozesManager(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public void RearrangeNewRepetitions(IDeck deck, IEnumerable<IUserRepetition> repetitions)
        {
            var activeRepetitions = repetitions.Take(deck.StartDelay);

            if (!activeRepetitions.Any(repetition => repetition.IsNew) && repetitions.Any(repetition => repetition.IsNew))
            {
                var newRepetition = repetitions.Where(repetition => repetition.IsNew).GetMinElement(repetition => repetition.Position);

                scheduler.MoveRepetition(repetitions, newRepetition.Position, deck.StartDelay, newRepetition.LastDelay, true, false);
            }

            Debug.Assert(Helpers.CheckPositions(repetitions));
        }
    }
}
