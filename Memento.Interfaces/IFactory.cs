using Memento.Models.Models;

namespace Memento.Interfaces
{
    public interface IFactory
    {
        Deck CreateDeck();
        Deck CreateDeck(int id);
        Deck CreateDeck(double coeff, int startDelay);

        Card CreateCard();
        Card CreateCard(Deck deck, string text, string comment, bool isValid);
    }
}
