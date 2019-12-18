using System.Collections.Generic;
using Neodenit.Memento.Common.DataModels;
using Neodenit.Memento.Common.Enums;

namespace Neodenit.Memento.Services.API
{
    public interface IClozesService
    {
        void AddClozes(Card card, IEnumerable<string> clozeNames);

        void PromoteCloze(Deck deck, Delays delay, string username);
    }
}