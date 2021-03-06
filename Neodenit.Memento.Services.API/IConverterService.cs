﻿using System;
using System.Collections.Generic;

namespace Neodenit.Memento.Services.API
{
    public interface IConverterService
    {
        string GetQuestion(string card, string clozeName);

        string GetFullAnswer(string card, string clozeName);

        string GetShortAnswer(string field, string clozeName);

        IEnumerable<Tuple<string, string>> GetCardsFromDeck(string deckText);

        IEnumerable<string> GetClozeNames(string field);

        string ReplaceAnswer(string text, string label, string newAnswers);

        string AddAltAnswer(string text, string label, string newAnswer);

        string FormatForExport(string text, string comment);

        string GetCurrentClozePattern(string clozeName);
    }
}