using System.Collections.Generic;
using Memento.Models.Models;

namespace Memento.Models.ViewModels
{
    public class DecksViewModel
    {
        public IEnumerable<Deck> UserDecks { get; set; }
        public IEnumerable<Deck> SharedDecks { get; set; }
    }
}
