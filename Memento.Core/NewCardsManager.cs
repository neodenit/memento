using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Core
{
    public static class NewCardsManager
    {
        public static void RearrangeNewCards(IDeck deck, IEnumerable<ICard> cards)
        {
            var activeCards = cards.Take(deck.StartDelay);

            if (!activeCards.Any(card => card.IsNew) && cards.Any(card => card.IsNew))
            {
                var newCard = cards.Where(card => card.IsNew).GetMinElement(card => card.Position);

                Scheduler.MoveCard(cards, newCard.Position, deck.StartDelay, newCard.LastDelay, true, false);
            }

            Debug.Assert(Helpers.CheckPositions(cards));
        }
    }
}
