using Memento.Core;
using Memento.DomainModel.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Memento.DomainModel.Models
{
    public class Card
    {
        [CheckCardOwner]
        public int ID { get; set; }

        public int DeckID { get; set; }

        public virtual Deck Deck { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        public virtual ICollection<Cloze> Clozes { get; set; }

        public bool IsValid { get; set; }

        public bool IsDeleted { get; set; }

        public Cloze GetNextCloze()
        {
            var cloze = Clozes.GetMinElement(item => item.Position);

            return cloze;
        }

        public bool IsAuthorized(IPrincipal user)
        {
            var userName = user.Identity.Name;

            return Deck.Owner == userName;
        }
    }
}
