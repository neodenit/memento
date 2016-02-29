namespace Memento.Interfaces
{
    public interface IDeckWithStatViewModel
    {
        IDeck Deck { get; set; }
        IStatistics Stat { get; set; }
    }
}