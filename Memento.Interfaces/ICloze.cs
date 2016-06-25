namespace Memento.Interfaces
{
    public interface ICloze
    {
        int CardID { get; set; }
        int ID { get; set; }
        string Label { get; set; }

        ICard GetCard();
        IUserRepetition GetUserRepetition(string username);
    }
}