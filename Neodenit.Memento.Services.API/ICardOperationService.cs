﻿namespace Neodenit.Memento.Services.API
{
    public interface ICardOperationService
    {
        string GetAnswerForCloze(string field, string clozeName);

        string GetCurrentClozePattern(string clozeName);

        string GetQuestionForCloze(string field, string clozeName);
    }
}