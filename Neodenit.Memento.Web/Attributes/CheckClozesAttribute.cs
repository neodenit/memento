using System.ComponentModel.DataAnnotations;
using System.Linq;
using Neodenit.Memento.Common.ViewModels;
using Neodenit.Memento.Services.API;

namespace Neodenit.Memento.Web.Attributes
{
    public class CheckClozesAttribute : ValidationAttribute
    {
        private readonly ValidationResult FailedValidationResult = new ValidationResult("Invalid");

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var validator = validationContext.GetService(typeof(IValidatorService)) as IValidatorService;
            var converter = validationContext.GetService(typeof(IConverterService)) as IConverterService;

            var card = (EditCardViewModel)value;

            var text = card.Text;
            var clozeNames = converter.GetClozeNames(text);

            ErrorMessage = validator.ErrorMessage;

            return clozeNames.Any() && clozeNames.All(clozeName => validator.Validate(text, clozeName))
                ? ValidationResult.Success
                : FailedValidationResult;
        }
    }
}
