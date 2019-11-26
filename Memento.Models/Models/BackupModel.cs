using System.Collections.Generic;

namespace Memento.Models.Models
{
    public class BackupModel
    {
        public IEnumerable<Deck> Decks { get; set; }

        public IEnumerable<Answer> Answers { get; set; }
    }
}
