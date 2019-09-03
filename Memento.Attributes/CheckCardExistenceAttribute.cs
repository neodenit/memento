using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Memento.Interfaces;

namespace Memento.Attributes
{
    public class CheckCardExistenceAttribute : ValidationAttribute
    {
        private IMementoRepository repository = DependencyResolver.Current.GetService<IMementoRepository>();

        public override bool IsValid(object value)
        {
            var cardID = (Guid)value;

            var card = repository.FindCard(cardID);

            return card != null;
        }
    }
}
