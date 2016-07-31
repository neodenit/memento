namespace Memento.Interfaces
{
    public interface IEditCardViewModel
    {
        int DeckID { get; set; }
        int ID { get; set; }
        string Text { get; set; }
        string Comment { get; set; }
    }
}