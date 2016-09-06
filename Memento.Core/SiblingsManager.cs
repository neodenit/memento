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

        public void RearrangeSiblings(IDeck deck, IEnumerable<IUserRepetition> repetitions)
        {
            var repetitionsToMove = GetRepetitionsToMove(deck, repetitions);

            MoveRepetitions(deck, repetitions, repetitionsToMove);

            Debug.Assert(Helpers.CheckPositions(repetitions));
        }

        private static IEnumerable<IUserRepetition> GetRepetitionsToMove(IDeck deck, IEnumerable<IUserRepetition> repetitions)
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
                return Enumerable.Empty<IUserRepetition>();
            }
        }

        private void MoveRepetitions(IDeck deck, IEnumerable<IUserRepetition> allRepetitions, IEnumerable<IUserRepetition> repetitionsToMove)
        {
            foreach (var repetition in repetitionsToMove)
            {
                scheduler.MoveRepetition(allRepetitions, repetition.Position, repetition.Position + deck.StartDelay, repetition.LastDelay + deck.StartDelay, true, false);
            }
        }
    }
}
