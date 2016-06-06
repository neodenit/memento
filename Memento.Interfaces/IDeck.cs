using System.Collections.Generic;

namespace Memento.Interfaces
{
    public enum ControlModes
    {
        Automatic,
        Manual,
    }

    public enum DelayModes
    {
        Smooth,
        Sharp,
        Combined,
    }

    public interface IDeck
    {
        bool AllowSmallDelays { get; set; }
        double Coeff { get; set; }
        ControlModes ControlMode { get; set; }
        DelayModes DelayMode { get; set; }
        int ID { get; set; }
        bool IsShared { get; set; }
        string Owner { get; set; }
        int StartDelay { get; set; }
        string Title { get; set; }

        IEnumerable<ICard> GetAllCards();
        IEnumerable<ICloze> GetClozes();
        IEnumerable<ICard> GetDeletedCards();
        IEnumerable<ICard> GetDraftCards();
        ICard GetNextCard();
        IEnumerable<ICard> GetValidCards();
    }
}