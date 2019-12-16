using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Neodenit.Memento.Interfaces;

namespace Neodenit.Memento.Web.Attributes
{
    public class CheckCardOwnerAttribute : ValidationAttribute
    {
        private readonly ValidationResult FailedValidationResult = new ValidationResult("Unauthorized");

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var cardService = validationContext.GetService(typeof(ICardsService)) as ICardsService;
            var deckService = validationContext.GetService(typeof(IDecksService)) as IDecksService;
            var httpContextAccessor = validationContext.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;

            var cardID = (Guid)value;

            var card = cardService.FindCard(cardID);

            if (card == null)
            {
                return ValidationResult.Success;
            }

            var deck = deckService.FindDeck(card.DeckID);

            return deck.Owner == httpContextAccessor.HttpContext.User.Identity.Name
                ? ValidationResult.Success
                : FailedValidationResult;
        }
    }
}
