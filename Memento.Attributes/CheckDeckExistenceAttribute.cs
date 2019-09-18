using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Memento.Interfaces;

namespace Memento.Attributes
{
    public class CheckDeckExistenceAttribute : ValidationAttribute
    {
        private IMementoRepository repository = DependencyResolver.Current.GetService<IMementoRepository>();

        public override bool IsValid(object value)
        {
            var deckID = (Guid)value;

            var deck = repository.FindDeck(deckID);

            return deck != null;
        }
    }
}
