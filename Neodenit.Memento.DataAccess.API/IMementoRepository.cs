﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neodenit.Memento.Common.Enums;
using Neodenit.Memento.Common.DataModels;

namespace Neodenit.Memento.DataAccess.API
{
    public interface IMementoRepository
    {
        Task<IEnumerable<Deck>> GetAllDecksAsync();

        Task<IEnumerable<Deck>> GetSharedDecksAsync();

        Task<IEnumerable<Deck>> GetUserDecksAsync(string userName);

        Task<IEnumerable<Answer>> GetAllAnswersAsync();

        Task<IEnumerable<Answer>> GetAnswersForDeckAsync(Guid deckID);

        Deck FindDeck(Guid id);

        Card FindCard(Guid id);

        Cloze FindCloze(Guid id);

        Task<Deck> FindDeckAsync(Guid id);

        Task<Card> FindCardAsync(Guid id);

        Task<Cloze> FindClozeAsync(Guid id);

        void AddDeck(Deck deck);

        void AddCard(Card card);

        void AddCloze(Cloze cloze);

        void RemoveDeck(Deck deck);

        void RemoveCard(Card card);

        void RemoveCloze(Cloze cloze);

        void RemoveDecks();

        void RemoveAnswers();

        void RemoveClozes(Card card, IEnumerable<string> clozeNames);

        void AddAnswer(Answer answer);

        void AddAnswer(Cloze cloze, bool isCorrect);

        Task SaveChangesAsync();

        IEnumerable<Cloze> GetClozes(Deck deck);

        IEnumerable<UserRepetition> GetRepetitions(Deck deck, string username);

        Card GetNextCard(Deck deck, string username);

        Cloze GetNextCloze(Card card, string username);

        IEnumerable<string> GetUsers(Card card);

        UserRepetition GetUserRepetition(Cloze cloze, string username);

        IEnumerable<string> GetUsers(Cloze cloze);
    }
}
