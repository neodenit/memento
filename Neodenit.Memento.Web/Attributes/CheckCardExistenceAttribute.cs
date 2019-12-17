using System;
using System.ComponentModel.DataAnnotations;
using Neodenit.Memento.Services.API;

namespace Neodenit.Memento.Web.Attributes
{
    public class CheckCardExistenceAttribute : ValidationAttribute
    {
        private readonly ValidationResult FailedValidationResult = new ValidationResult("NotFound");

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var service = validationContext.GetService(typeof(ICardsService)) as ICardsService;

            var cardID = (Guid)value;

            var card = service.FindCard(cardID);

            return card != null ? ValidationResult.Success : FailedValidationResult;
        }
    }
}
