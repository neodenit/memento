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
    public class SiblingsManager : ISiblingsManager
    {
        private readonly IScheduler scheduler;

        public SiblingsManager(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public void RearrangeSiblings(IDeck deck, IEnumerable<ICloze> clozes, string username)
        {
            var clozesToMove = GetClozesToMove(deck, clozes, username);

            MoveClozes(deck, clozes, clozesToMove, username);

            Debug.Assert(Helpers.CheckPositions(clozes, username));
        }

        private static IEnumerable<ICloze> GetClozesToMove(IDeck deck, IEnumerable<ICloze> clozes, string username)
        {
            var cardID = clozes.GetMinElement(cloze => cloze.GetUserRepetition(username).Position).CardID;

            var nextClozes = clozes.Skip(1).Take(deck.StartDelay);

            if (nextClozes.All(cloze => cloze.CardID == cardID))
            {
                return clozes.Skip(1).TakeWhile(cloze => cloze.CardID == cardID);
            }
            else if (nextClozes.Any(cloze => cloze.CardID == cardID))
            {
                return nextClozes.Where(cloze => cloze.CardID == cardID);
            }
            else
            {
                return Enumerable.Empty<ICloze>();
            }
        }

        private void MoveClozes(IDeck deck, IEnumerable<ICloze> allClozes, IEnumerable<ICloze> clozesToMove, string username)
        {
            foreach (var cloze in clozesToMove)
            {
                scheduler.MoveCloze(allClozes, cloze.GetUserRepetition(username).Position, cloze.GetUserRepetition(username).Position + deck.StartDelay, cloze.GetUserRepetition(username).LastDelay + deck.StartDelay, true, false, username);
            }
        }
    }
}
