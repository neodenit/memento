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

        public void RearrangeNewClozes(IDeck deck, IEnumerable<ICloze> clozes)
        {
            var activeClozes = clozes.Take(deck.StartDelay);

            if (!activeClozes.Any(cloze => cloze.IsNew) && clozes.Any(cloze => cloze.IsNew))
            {
                var newCloze = clozes.Where(cloze => cloze.IsNew).GetMinElement(cloze => cloze.Position);

                scheduler.MoveCloze(clozes, newCloze.Position, deck.StartDelay, newCloze.LastDelay, true, false);
            }

            Debug.Assert(Helpers.CheckPositions(clozes));
        }
    }
}
