using System;

namespace Memento.Interfaces
{
    public interface IAnswer
    {
        int CardID { get; set; }
        int ClozeID { get; set; }
        int DeckID { get; set; }
        int ID { get; set; }
        bool IsCorrect { get; set; }
        string Owner { get; set; }
        DateTime Time { get; set; }
    }
}