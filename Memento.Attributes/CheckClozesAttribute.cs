using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Memento.Interfaces;
using Memento.Models.ViewModels;

namespace Memento.Attributes
{
    public class CheckClozesAttribute : ValidationAttribute
    {
        private IValidatorService validator = DependencyResolver.Current.GetService<IValidatorService>();
        private IConverterService converter = DependencyResolver.Current.GetService<IConverterService>();

        public override bool IsValid(object value)
        {
            var card = (EditCardViewModel)value;

            var text = card.Text;
            var clozeNames = converter.GetClozeNames(text);

            ErrorMessage = validator.ErrorMessage;

            return clozeNames.Any() && clozeNames.All(clozeName => validator.Validate(text, clozeName));
        }
    }
}
