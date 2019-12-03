using System;
using System.ComponentModel.DataAnnotations;
using Neodenit.Memento.Interfaces;

namespace Neodenit.Memento.Attributes
{
    public class CheckCardExistenceAttribute : ValidationAttribute
    {
        private readonly ValidationResult FailedValidationResult = new ValidationResult("NotFound");

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var repository = validationContext.GetService(typeof(IMementoRepository)) as IMementoRepository;

            var cardID = (Guid)value;

            var card = repository.FindCard(cardID);

            return card != null ? ValidationResult.Success : FailedValidationResult;
        }
    }
}
