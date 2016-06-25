namespace Memento.Interfaces
{
    public interface IUserRepetition
    {
        int ClozeID { get; set; }
        int ID { get; set; }
        bool IsNew { get; set; }
        int LastDelay { get; set; }
        int Position { get; set; }
        string UserName { get; set; }
    }
}