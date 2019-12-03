using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Neodenit.Memento.Interfaces;

namespace Neodenit.Memento.Attributes
{
    public class CheckCardOwnerAttribute : ValidationAttribute
    {
        private readonly ValidationResult FailedValidationResult = new ValidationResult("Unauthorized");

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var repository = validationContext.GetService(typeof(IMementoRepository)) as IMementoRepository;
            var httpContextAccessor = validationContext.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;

            var cardID = (Guid)value;

            var card = repository.FindCard(cardID);

            return card == null || card.GetDeck().Owner == httpContextAccessor.HttpContext.User.Identity.Name
                ? ValidationResult.Success
                : FailedValidationResult;
        }
    }
}
