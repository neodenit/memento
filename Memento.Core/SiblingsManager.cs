using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Core
{
    public static class SiblingsManager
    {
        public static void RearrangeSiblings(IDeck deck, IEnumerable<ICard> cards)
        {
            var cardsToMove = GetCardsToMove(deck, cards);

            MoveCards(deck, cards, cardsToMove);

            Debug.Assert(Helpers.CheckPositions(cards));
        }

        private static IEnumerable<ICard> GetCardsToMove(IDeck deck, IEnumerable<ICard> cards)
        {
            var cardID = cards.GetMinElement(card => card.Position).CardID;

            var nextCards = cards.Skip(1).Take(deck.StartDelay);

            if (nextCards.All(card => card.CardID == cardID))
            {
                return cards.Skip(1).TakeWhile(card => card.CardID == cardID);
            }
            else if (nextCards.Any(card => card.CardID == cardID))
            {
                return nextCards.Where(card => card.CardID == cardID);
            }
            else
            {
                return Enumerable.Empty<ICard>();
            }
        }

        private static void MoveCards(IDeck deck, IEnumerable<ICard> allCards, IEnumerable<ICard> cardsToMove)
        {
            foreach (var card in cardsToMove)
            {
                Scheduler.MoveCard(allCards, card.Position, card.Position + deck.StartDelay, card.LastDelay + deck.StartDelay, true, false);
            }
        }
    }
}
