﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neodenit.Memento.Additional;
using Neodenit.Memento.Models.DataModels;
using Neodenit.Memento.Models.ViewModels;

namespace Neodenit.Memento.Interfaces
{
    public interface IDecksService
    {
        Task<IEnumerable<Deck>> GetDecksAsync(string username);

        Task<IEnumerable<Deck>> GetSharedDecksAsync();

        Task<DeckWithStatViewModel> GetDeckWithStatViewModel(Guid deckID, Statistics statistics, string username);

        Task<Deck> FindDeckAsync(Guid id);

        Task UpdateDeck(Guid id, string title, int startDelay, double coeff, bool previewAnswer);

        Task CreateDeck(Deck deck, string userName);

        Task DeleteDeck(Guid id);

        Task ShareDeckAsync(Guid id);
    }
}