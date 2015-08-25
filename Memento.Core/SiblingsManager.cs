using System;
using System.Collections.Generic;
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
        }

        private static IEnumerable<ICard> GetCardsToMove(IDeck deck, IEnumerable<ICard> cards)
        {
            var topicID = cards.GetMinElement(card => card.Position).TopicID;

            var nextCards = cards.Skip(1).Take(deck.StartDelay);

            if (nextCards.All(card => card.TopicID == topicID))
            {
                return cards.Skip(1).TakeWhile(card => card.TopicID == topicID);
            }
            else if (nextCards.Any(card => card.TopicID == topicID))
            {
                return nextCards.Where(card => card.TopicID == topicID);
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
