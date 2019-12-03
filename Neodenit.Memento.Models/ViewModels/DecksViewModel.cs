using System.Collections.Generic;
using Neodenit.Memento.Models.DataModels;

namespace Neodenit.Memento.Models.ViewModels
{
    public class DecksViewModel
    {
        public IEnumerable<Deck> UserDecks { get; set; }

        public IEnumerable<Deck> SharedDecks { get; set; }
    }
}
