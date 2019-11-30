using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Memento.Common;
using Memento.Interfaces;
using Memento.Models.Helpers;
using Memento.Models.Models;

namespace Memento.Services
{
    public class SiblingsManagerService : ISiblingsManagerService
    {
        private readonly ISchedulerOperationService scheduler;

        public SiblingsManagerService(ISchedulerOperationService scheduler)
        {
            this.scheduler = scheduler;
        }

        public void RearrangeSiblings(Deck deck, IEnumerable<UserRepetition> repetitions)
        {
            var repetitionsToMove = GetRepetitionsToMove(deck, repetitions);

            MoveRepetitions(deck, repetitions, repetitionsToMove);

            Debug.Assert(ModelHelpers.ArePositionsValid(repetitions));
        }

        private static IEnumerable<UserRepetition> GetRepetitionsToMove(Deck deck, IEnumerable<UserRepetition> repetitions)
        {
            var cardID = repetitions.GetMinElement(repetition => repetition.Position).GetCloze().CardID;

            var nextRepetitions = repetitions.Skip(1).Take(deck.StartDelay);

            if (nextRepetitions.All(repetition => repetition.GetCloze().CardID == cardID))
            {
                return repetitions.Skip(1).TakeWhile(repetition => repetition.GetCloze().CardID == cardID);
            }
            else if (nextRepetitions.Any(repetition => repetition.GetCloze().CardID == cardID))
            {
                return nextRepetitions.Where(repetition => repetition.GetCloze().CardID == cardID);
            }
            else
            {
                return Enumerable.Empty<UserRepetition>();
            }
        }

        private void MoveRepetitions(Deck deck, IEnumerable<UserRepetition> allRepetitions, IEnumerable<UserRepetition> repetitionsToMove)
        {
            foreach (var repetition in repetitionsToMove)
            {
                scheduler.MoveRepetition(allRepetitions, repetition.Position, repetition.Position + deck.StartDelay, repetition.LastDelay + deck.StartDelay, true, false);
            }
        }
    }
}
