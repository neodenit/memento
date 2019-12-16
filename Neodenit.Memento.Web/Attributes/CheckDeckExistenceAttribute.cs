using System;
using System.ComponentModel.DataAnnotations;
using Neodenit.Memento.Interfaces;

namespace Neodenit.Memento.Web.Attributes
{
    public class CheckDeckExistenceAttribute : ValidationAttribute
    {
        private readonly ValidationResult FailedValidationResult = new ValidationResult("NotFound");

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var service = validationContext.GetService(typeof(IDecksService)) as IDecksService;

            var deckID = (Guid)value;

            var deck = service.FindDeck(deckID);

            return deck != null ? ValidationResult.Success : FailedValidationResult;
        }
    }
}
