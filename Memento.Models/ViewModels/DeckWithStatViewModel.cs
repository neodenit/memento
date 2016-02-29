using Memento.Interfaces;

namespace Memento.Models.ViewModels
{
    public class DeckWithStatViewModel : IDeckWithStatViewModel
    {
        public IDeck Deck { get; set; }
        public IStatistics Stat { get; set; }
    }
}