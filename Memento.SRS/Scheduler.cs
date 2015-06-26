using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.SRS
{
    public static class Scheduler
    {
        public enum Delays
        {
            AfterNext,
            Next,
            Same,
            Previous,
            Initial,
        }

        public static void PromoteCard(IDeck deck, IEnumerable<ICard> cards, Delays delay)
        {
            var card = GetFirstCard(cards);

            var maxNewPosition = GetMaxNewPosition(cards);

            var step = GetStep(deck, delay, card.LastDelay);

            Debug.Assert(step > 0, "Step value is negative.");

            var newPosition = Math.Min(step, maxNewPosition);

            var newDelay = deck.AllowSmallDelays ? newPosition : Math.Max(newPosition, deck.StartDelay);

            ChangeFirstCardPosition(cards, deck.CorrectDelays, card, newPosition, newDelay);

            card.IsNew = false;
        }

        public static void AddCard(IDeck deck, ICollection<ICard> cards, ICard card)
        {
            PrepareForAdding(deck, cards, card);

            cards.Add(card);
        }

        public static void PrepareForAdding(IDeck deck, IEnumerable<ICard> cards, ICard card)
        {
            var maxNewPosition = GetMaxNewPosition(cards);
            
            card.Position = maxNewPosition;

            card.IsNew = true;
        }

        public static void ShuffleNewCards(IEnumerable<ICard> cards)
        {
            var newCards = from item in cards where item.IsNew select item;

            ShuffleCards(newCards);
        }

        private static void ShuffleCards(IEnumerable<ICard> cards)
        {
            var positions = from item in cards select item.Position;

            var shuffledNumbers = positions.OrderBy(item => Guid.NewGuid());

            var zip = Enumerable.Zip(cards, shuffledNumbers, (card, newPos) => new { card, newPos });

            zip.ToList().ForEach(item => item.card.Position = item.newPos);
        }

        private static void ChangeFirstCardPosition(IEnumerable<ICard> cards, bool correctDelays, ICard card, int newPosition, int newDelay)
        {
            DecreasePositions(cards, newPosition);

            if (correctDelays)
            {
                CorrectDelays(cards, newPosition);
            }

            card.Position = newPosition;
            card.LastDelay = newDelay;
        }

        private static void IncreasePositions(IEnumerable<ICard> cards, int position)
        {
            var more = from item in cards where item.Position >= position select item;

            foreach (var item in more)
            {
                item.Position++;
            }
        }

        private static void DecreasePositions(IEnumerable<ICard> cards, int position)
        {
            var less = from item in cards where item.Position <= position select item;

            foreach (var item in less)
            {
                item.Position--;
            }
        }

        private static void CorrectDelays(IEnumerable<ICard> cards, int newPosition)
        {
            var more = from item in cards where item.Position > newPosition && !item.IsNew select item;

            foreach (var item in more)
            {
                item.LastDelay++;
            }
        }

        private static int GetMaxNewPosition(IEnumerable<ICard> cards)
        {
            var max = cards.Max(item => item.Position);
            var nextToMax = max + 1;
            return nextToMax;
        }

        private static ICard GetFirstCard(IEnumerable<ICard> cards)
        {
            var card = cards.GetMinElement(item => item.Position);
            return card;
        }

        private static void ReservePosition(IEnumerable<ICard> cards, int position, bool correctDelays)
        {
            IncreasePositions(cards, position);

            if (correctDelays)
            {
                CorrectDelays(cards, position);
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
