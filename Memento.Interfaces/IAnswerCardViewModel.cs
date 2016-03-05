using Memento.Interfaces;

namespace Memento.Interfaces
{
    public interface IAnswerCardViewModel
    {
        int ID { get; set; }
        string Text { get; set; }
        string Question { get; set; }
        string Answer { get; set; }
        Mark Mark { get; set; }
        int DeckID { get; set; }
        string DeckTitle { get; set; }
    }
}