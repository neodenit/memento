using System.Collections.Generic;
using Neodenit.Memento.Common.Enums;
using Neodenit.Memento.DataAccess.API.DataModels;

namespace Neodenit.Memento.Interfaces
{
    public interface ISchedulerUtilsService
    {
        void DecreaseDelays(IEnumerable<UserRepetition> repetitions);

        void DecreasePosition(IEnumerable<UserRepetition> repetitions);

        UserRepetition GetFirstRepetition(IEnumerable<UserRepetition> repetitions);

        int GetMaxNewPosition(IEnumerable<UserRepetition> repetitions);

        int GetMaxPosition(IEnumerable<UserRepetition> repetitions);

        int GetRandomPart(int minStep = 0, int maxStep = 1);

        IEnumerable<UserRepetition> GetRange(IEnumerable<UserRepetition> repetitions, int minPosition, int maxPosition);

        IEnumerable<UserRepetition> GetRestRepetitions(IEnumerable<UserRepetition> repetitions, int position);

        int GetStep(Deck deck, Delays delay, int lastDelay);

        void IncreaseDelays(IEnumerable<UserRepetition> repetitions);

        void IncreasePosition(IEnumerable<UserRepetition> repetitions);

        void ReservePosition(IEnumerable<UserRepetition> repetitions, int position, bool correctDelays);

        void ShuffleRepetitions(IEnumerable<UserRepetition> repetitions);
    }
}