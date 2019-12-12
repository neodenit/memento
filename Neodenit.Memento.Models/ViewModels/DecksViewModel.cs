using System.Collections.Generic;

namespace Neodenit.Memento.Models.ViewModels
{
    public class DecksViewModel
    {
        public IEnumerable<DeckViewModel> UserDecks { get; set; }

        public IEnumerable<DeckViewModel> SharedDecks { get; set; }
    }
}
