namespace Memento.Interfaces
{
    public interface ICloze
    {
        int CardID { get; set; }
        int ID { get; set; }
        bool IsNew { get; set; }
        string Label { get; set; }
        int LastDelay { get; set; }
        int Position { get; set; }

        ICard GetCard();
    }
}