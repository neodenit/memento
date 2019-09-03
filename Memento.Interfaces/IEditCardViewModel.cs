using System;

namespace Memento.Interfaces
{
    public interface IEditCardViewModel
    {
        int DeckID { get; set; }
        Guid ID { get; set; }
        string Text { get; set; }
        string Comment { get; set; }
    }
}