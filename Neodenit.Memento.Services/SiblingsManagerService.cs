using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Neodenit.Memento.Common;
using Neodenit.Memento.DataAccess.API.DataModels;
using Neodenit.Memento.Interfaces;

namespace Neodenit.Memento.Services
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
            var cardID = repetitions.GetMinElement(repetition => repetition.Position).Cloze.CardID;

            var nextRepetitions = repetitions.Skip(1).Take(deck.StartDelay);

            if (nextRepetitions.All(repetition => repetition.Cloze.CardID == cardID))
            {
                return repetitions.Skip(1).TakeWhile(repetition => repetition.Cloze.CardID == cardID);
            }
            else if (nextRepetitions.Any(repetition => repetition.Cloze.CardID == cardID))
            {
                return nextRepetitions.Where(repetition => repetition.Cloze.CardID == cardID);
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
