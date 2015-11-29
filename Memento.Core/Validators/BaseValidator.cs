using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Memento.Core.Validators
{
    public abstract class BaseValidator : IValidator
    {
        private readonly IValidator baseValidator;

        public BaseValidator(IValidator baseValidator = null)
        {
            this.baseValidator = baseValidator;
        }

        public bool Validate(string field, string clozeName)
        {
            return ValidateBase(field, clozeName) && ValidateThis(field, clozeName);
        }

        protected abstract bool ValidateThis(string field, string clozeName);

        private bool ValidateBase(string field, string clozeName)
        {
            return baseValidator == null || baseValidator.Validate(field, clozeName);
        }
    }
}
