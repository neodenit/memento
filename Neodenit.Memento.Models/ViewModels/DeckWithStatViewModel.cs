using Neodenit.Memento.Additional;
using Neodenit.Memento.Models.DataModels;

namespace Neodenit.Memento.Models.ViewModels
{
    public class DeckWithStatViewModel
    {
        public Deck Deck { get; set; }

        public Statistics Stat { get; set; }
    }
}