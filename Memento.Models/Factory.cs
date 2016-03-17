using Memento.Interfaces;
using Memento.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Models
{
    public class Factory : IFactory
    {
        public IDeck CreateDeck() =>
            new Deck();

        public IDeck CreateDeck(int id) =>
            new Deck { ID = id };

        public IDeck CreateDeck(double coeff, int startDelay) =>
            new Deck { Coeff = coeff, StartDelay = startDelay };

        public ICard CreateCard() =>
            new Card();

        public ICard CreateCard(IDeck deck, string text, bool isValid) =>
            new Card(deck, text, isValid);
    }
}
