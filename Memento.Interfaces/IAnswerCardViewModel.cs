using Memento.Interfaces;

namespace Memento.Interfaces
{
    public interface IAnswerCardViewModel
    {
        int ID { get; set; }

        string Question { get; set; }

        string FullAnswer { get; set; }

        string ShortAnswer { get; set; }

        string UserAnswer { get; set; }

        string Comment { get; set; }

        Mark Mark { get; set; }

        DelayModes DelayMode { get; set; }

        string DeckTitle { get; set; }

        IDeckWithStatViewModel Statistics { get; set; }
    }
}