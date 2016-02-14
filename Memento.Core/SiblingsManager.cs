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

        public void RearrangeSiblings(IDeck deck, IEnumerable<ICloze> clozes)
        {
            var clozesToMove = GetClozesToMove(deck, clozes);

            MoveClozes(deck, clozes, clozesToMove);

            Debug.Assert(Helpers.CheckPositions(clozes));
        }

        private static IEnumerable<ICloze> GetClozesToMove(IDeck deck, IEnumerable<ICloze> clozes)
        {
            var cardID = clozes.GetMinElement(cloze => cloze.Position).CardID;

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

        private void MoveClozes(IDeck deck, IEnumerable<ICloze> allClozes, IEnumerable<ICloze> clozesToMove)
        {
            foreach (var cloze in clozesToMove)
            {
                scheduler.MoveCloze(allClozes, cloze.Position, cloze.Position + deck.StartDelay, cloze.LastDelay + deck.StartDelay, true, false);
            }
        }
    }
}
