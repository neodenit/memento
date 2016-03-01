using Memento.Additional;
using Memento.Common;
using Memento.Interfaces;
using Memento.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Services
{
    public class CardsService : ICardsService
    {
        private readonly IMementoRepository repository;

        public CardsService(IMementoRepository repository)
        {
            this.repository = repository;
        }

        public async Task<ICard> GetNextCardAsync(int deckID)
        {
            var dbDeck = await repository.FindDeckAsync(deckID);

            if (dbDeck.GetValidCards().Any())
            {
                return dbDeck.GetNextCard();
            }
            else
            {
                return null;
            }
        }
    }
}
