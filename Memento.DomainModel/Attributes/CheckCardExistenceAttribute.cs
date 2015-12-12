using Memento.DomainModel.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Memento.DomainModel.Attributes
{
    public class CheckCardExistenceAttribute : ValidationAttribute
    {
        private IMementoRepository repository = DependencyResolver.Current.GetService<IMementoRepository>();

        public override bool IsValid(object value)
        {
            var cardID = (int)value;

            var card = repository.FindCard(cardID);

            return card != null;
        }
    }
}
