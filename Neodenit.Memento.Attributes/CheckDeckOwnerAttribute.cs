using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Neodenit.Memento.DataAccess.API;

namespace Neodenit.Memento.Attributes
{
    public class CheckDeckOwnerAttribute : ValidationAttribute
    {
        private readonly ValidationResult FailedValidationResult = new ValidationResult("Unauthorized");

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var repository = validationContext.GetService(typeof(IMementoRepository)) as IMementoRepository;
            var httpContextAccessor = validationContext.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;

            var deckID = (Guid)value;

            var deck = repository.FindDeck(deckID);

            return deck == null || deck.Owner == httpContextAccessor.HttpContext.User.Identity.Name
                ? ValidationResult.Success
                : FailedValidationResult;
        }
    }
}
