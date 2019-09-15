using Memento.Interfaces;
using Memento.Models.Models;

namespace Memento.Core
{
    public class Factory : IFactory
    {
        public Deck CreateDeck() =>
            new Deck();

        public Deck CreateDeck(int id) =>
            new Deck { ID = id };

        public Deck CreateDeck(double coeff, int startDelay) =>
            new Deck { Coeff = coeff, StartDelay = startDelay };

        public Card CreateCard() =>
            new Card();

        public Card CreateCard(Deck deck, string text, string comment, bool isValid) =>
            new Card(deck, text, comment, isValid);
    }
}
