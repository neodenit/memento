using Memento.Common;
using Memento.Attributes;
using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Models.Models
{
    public class Card : ICard
    {
        [CheckCardOwner]
        public int ID { get; set; }

        public int DeckID { get; set; }

        public virtual Deck Deck { get; set; }

        public IDeck GetDeck()
        {
            return Deck;
        }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        public virtual ICollection<Cloze> Clozes { get; set; }

        public IEnumerable<ICloze> GetClozes()
        {
            return Clozes;
        }

        public void AddCloze(ICloze cloze)
        {
            Clozes.Add(cloze as Cloze);
        }

        public bool IsValid { get; set; }

        public bool IsDeleted { get; set; }

        public ICloze GetNextCloze()
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
