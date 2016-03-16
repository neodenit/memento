using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface IFactory
    {
        IDeck CreateDeck();
        IDeck CreateDeck(int id);
        IDeck CreateDeck(double coeff, int startDelay);

        ICard CreateCard();
    }
}
