using System.Collections.Generic;

namespace Neodenit.Memento.Models.DataModels
{
    public class BackupModel
    {
        public IEnumerable<Deck> Decks { get; set; }

        public IEnumerable<Answer> Answers { get; set; }
    }
}
