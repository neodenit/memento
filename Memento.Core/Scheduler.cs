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
    public class Scheduler : IScheduler
    {
        public void PromoteCard(IDeck deck, IEnumerable<ICard> cards, Delays delay)
        {
            var card = GetFirstCard(cards);

            var maxNewPosition = GetMaxPosition(cards);

            var step = GetStep(deck, delay, card.LastDelay);

            Debug.Assert(step > 0, "Step value is negative.");

            var newPosition = Math.Min(step, maxNewPosition);

            var newDelay = newPosition > deck.StartDelay || deck.AllowSmallDelays ? newPosition : deck.StartDelay;

            MoveCard(cards, card.Position, newPosition, newDelay, false, true);

            card.IsNew = false;
        }

        public void AddCard(IDeck deck, ICollection<ICard> cards, ICard card)
        {
            PrepareForAdding(deck, cards, card);

            cards.Add(card);
        }

        public void PrepareForAdding(IDeck deck, IEnumerable<ICard> cards, ICard card)
        {
            var maxNewPosition = GetMaxNewPosition(cards);

            card.Position = maxNewPosition;

            card.LastDelay = deck.StartDelay;

            card.IsNew = true;
        }

        public void PrepareForRemoving(IDeck deck, IEnumerable<ICard> cards, ICard card)
        {
            var position = card.Position;

            var movedCards = GetRestCards(cards, position);

            DecreasePosition(movedCards);

            DecreaseDelays(movedCards);
        }

        public void ShuffleNewCards(IEnumerable<ICard> cards)
        {
            var newCards = from item in cards where item.IsNew select item;

            ShuffleCards(newCards);
        }

        public void MoveCard(IEnumerable<ICard> cards, int oldPosition, int newPosition, int newDelay, bool correctMovedCardsDelays, bool correctRestCardsDelays)
        {
            var movedCard = cards.Single(item => item.Position == oldPosition);

            movedCard.Position = -1;

            var newLimitedPosition =
                oldPosition > newPosition ?
                Math.Max(newPosition, 0) :
                Math.Min(newPosition, GetMaxPosition(cards));

            if (oldPosition > newPosition)
            {
                var movedCards = GetRange(cards, newLimitedPosition, oldPosition - 1);

                IncreasePosition(movedCards);

                if (correctMovedCardsDelays)
                {
                    IncreaseDelays(movedCards);
                }
            }
            else
            {
                var movedCards = GetRange(cards, oldPosition + 1, newLimitedPosition);

                DecreasePosition(movedCards);

                if (correctMovedCardsDelays)
                {
                    DecreaseDelays(movedCards);
                }

                if (correctRestCardsDelays)
                {
                    var restCards = GetRestCards(cards, newLimitedPosition);

                    IncreaseDelays(restCards);
                }
            }

            movedCard.Position = newLimitedPosition;
            movedCard.LastDelay = newDelay;
        }

        private static void ShuffleCards(IEnumerable<ICard> cards)
        {
            var positions = from item in cards select item.Position;

            var shuffledNumbers = positions.OrderBy(item => Guid.NewGuid());

            var zip = Enumerable.Zip(cards, shuffledNumbers, (card, newPos) => new { card, newPos });

            zip.ToList().ForEach(item => item.card.Position = item.newPos);
        }

        private static IEnumerable<ICard> GetRestCards(IEnumerable<ICard> cards, int position)
        {
            var result = from item in cards where item.Position >= position select item;

            return result;
        }

        private static IEnumerable<ICard> GetRange(IEnumerable<ICard> cards, int minPosition, int maxPosition)
        {
            var result = from card in cards
                         where card.Position >= minPosition && card.Position <= maxPosition
                         select card;

            return result;
        }

        private static void IncreasePosition(IEnumerable<ICard> cards)
        {
            foreach (var item in cards)
            {
                item.Position++;
            }
        }

        private static void DecreasePosition(IEnumerable<ICard> cards)
        {
            foreach (var item in cards)
            {
                item.Position--;
            }
        }

        private static void IncreaseDelays(IEnumerable<ICard> cards)
        {
            foreach (var card in cards)
            {
                if (!card.IsNew)
                {
                    card.LastDelay++;
                }
            }
        }

        private static void DecreaseDelays(IEnumerable<ICard> cards)
        {
            foreach (var card in cards)
            {
                if (!card.IsNew)
                {
                    card.LastDelay--;
                }
            }
        }

        private static int GetMaxPosition(IEnumerable<ICard> cards)
        {
            return cards.Any() ? cards.Max(item => item.Position) : 0;
        }

        private static int GetMaxNewPosition(IEnumerable<ICard> cards)
        {
            if (cards.Any())
            {
                var max = cards.Max(item => item.Position);
                var nextToMax = max + 1;
                return nextToMax;
            }
            else
            {
                return 0;
            }
        }

        private static ICard GetFirstCard(IEnumerable<ICard> cards)
        {
            var card = cards.GetMinElement(item => item.Position);

            return card;
        }

        [Obsolete]
        private static void ReservePosition(IEnumerable<ICard> cards, int position, bool correctDelays)
        {
            var movedCards = from item in cards where item.Position >= position select item;

            IncreasePosition(movedCards);

            if (correctDelays)
            {
                IncreaseDelays(movedCards);
            }
        }

        private static int GetStep(IDeck deck, Delays delay, int lastDelay)
        {
            Func<double, int> op = Utils.Round;

            switch (delay)
            {
                case Delays.Initial:
                    return op(deck.StartDelay);
                case Delays.Previous:
                    return op(lastDelay / deck.Coeff);
                case Delays.Same:
                    return lastDelay;
                case Delays.Next:
                    return op(lastDelay * deck.Coeff);
                case Delays.AfterNext:
                    return op(lastDelay * deck.Coeff * deck.Coeff);
                default:
                    return -1;
            }
        }
    }
}
