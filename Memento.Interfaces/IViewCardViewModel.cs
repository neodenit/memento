using System;

namespace Memento.Interfaces
{
    public interface IViewCardViewModel
    {
        Guid ID { get; set; }
        string Text { get; set; }
        string DeckTitle { get; set; }
    }
}