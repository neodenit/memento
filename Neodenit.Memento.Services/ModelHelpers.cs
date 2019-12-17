using System.Collections.Generic;
using System.Linq;
using Neodenit.Memento.Common.DataModels;

namespace Neodenit.Memento.Services
{
    public class ModelHelpers
    {
        public static bool ArePositionsValid(IEnumerable<UserRepetition> repetitions)
        {
            var repetitionPositions = from r in repetitions orderby r.Position select r.Position;
            var n = repetitions.Count();
            var result = repetitionPositions.SequenceEqual(Enumerable.Range(0, n));

            return result;
        }
    }
}
