using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using Memento.Interfaces;

namespace Memento.Attributes
{
    public class CheckDeckOwnerAttribute : ValidationAttribute
    {
        private IMementoRepository repository = DependencyResolver.Current.GetService<IMementoRepository>();

        public override bool IsValid(object value)
        {
            var deckID = (Guid)value;

            var deck = repository.FindDeck(deckID);

            return deck == null || deck.Owner == HttpContext.Current.User.Identity.Name;
        }
    }
}
