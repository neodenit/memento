using System.Collections.Generic;
using Neodenit.Memento.Common.Enums;
using Neodenit.Memento.Common.DataModels;

namespace Neodenit.Memento.Services.API
{
    public interface ISchedulerOperationService
    {
        void MoveRepetition(IEnumerable<UserRepetition> repetitions, int oldPosition, int newPosition, int newDelay, bool correctMovedRepetitionsDelays, bool correctRestRepetitionsDelays);

        void PrepareForAdding(Deck deck, IEnumerable<UserRepetition> repetitions, UserRepetition repetition);

        void PrepareForRemoving(Deck deck, IEnumerable<UserRepetition> repetitions, UserRepetition repetition);

        void PromoteRepetition(Deck deck, IEnumerable<UserRepetition> repetitions, Delays delay);

        void ShuffleNewRepetitions(IEnumerable<UserRepetition> repetitions);
    }
}