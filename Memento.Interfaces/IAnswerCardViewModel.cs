﻿using Memento.Interfaces;

namespace Memento.Interfaces
{
    public interface IAnswerCardViewModel
    {
        int ID { get; set; }

        string Question { get; set; }
        string CorrectAnswer { get; set; }
        string UserAnswer { get; set; }

        Mark Mark { get; set; }
        DelayModes DelayMode { get; set; }

        string DeckTitle { get; set; }
    }
}