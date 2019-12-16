using System;
using System.ComponentModel.DataAnnotations;
using Neodenit.Memento.DataAccess.API;

namespace Neodenit.Memento.Attributes
{
    public class CheckDeckExistenceAttribute : ValidationAttribute
    {
        private readonly ValidationResult FailedValidationResult = new ValidationResult("NotFound");

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var repository = validationContext.GetService(typeof(IMementoRepository)) as IMementoRepository;

            var deckID = (Guid)value;

            var deck = repository.FindDeck(deckID);

            return deck != null ? ValidationResult.Success : FailedValidationResult;
        }
    }
}
