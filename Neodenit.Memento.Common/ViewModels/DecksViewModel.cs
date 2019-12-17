using System.Collections.Generic;

namespace Neodenit.Memento.Common.ViewModels
{
    public class DecksViewModel
    {
        public IEnumerable<DeckViewModel> UserDecks { get; set; }

        public IEnumerable<DeckViewModel> SharedDecks { get; set; }
    }
}
