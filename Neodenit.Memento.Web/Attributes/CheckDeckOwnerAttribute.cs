using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Neodenit.Memento.Interfaces;

namespace Neodenit.Memento.Web.Attributes
{
    public class CheckDeckOwnerAttribute : ValidationAttribute
    {
        private readonly ValidationResult FailedValidationResult = new ValidationResult("Unauthorized");

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var service = validationContext.GetService(typeof(IDecksService)) as IDecksService;
            var httpContextAccessor = validationContext.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;

            var deckID = (Guid)value;

            var deck = service.FindDeck(deckID);

            return deck == null || deck.Owner == httpContextAccessor.HttpContext.User.Identity.Name
                ? ValidationResult.Success
                : FailedValidationResult;
        }
    }
}
