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

        public void RearrangeNewClozes(IDeck deck, IEnumerable<ICloze> clozes, string username)
        {
            var activeClozes = clozes.Take(deck.StartDelay);

            if (!activeClozes.Any(cloze => cloze.GetUserRepetition(username).IsNew) && clozes.Any(cloze => cloze.GetUserRepetition(username).IsNew))
            {
                var newCloze = clozes.Where(cloze => cloze.GetUserRepetition(username).IsNew).GetMinElement(cloze => cloze.GetUserRepetition(username).Position);

                scheduler.MoveCloze(clozes, newCloze.GetUserRepetition(username).Position, deck.StartDelay, newCloze.GetUserRepetition(username).LastDelay, true, false, username);
            }

            Debug.Assert(Helpers.CheckPositions(clozes, username));
        }
    }
}
