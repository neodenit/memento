using Memento.Core;
using Memento.Core.Validators;
using Memento.DomainModel.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Memento.DomainModel.Attributes
{
    public class CheckClozesAttribute : ValidationAttribute
    {
        private IValidator validator = DependencyResolver.Current.GetService<IValidator>();
        private IConverter converter = DependencyResolver.Current.GetService<IConverter>();

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
