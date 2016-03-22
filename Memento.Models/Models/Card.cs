using Memento.Common;
using Memento.Attributes;
using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Models.Models
{
    public class Card : ICard
    {
        public Card()
        {
            Clozes = new List<Cloze>();
        }

        public Card(IDeck deck, string text, string comment, bool isValid) : this()
        {
            Deck = deck as Deck;
            Text = text;
            IsValid = isValid;
            Comment = comment;
        }

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

        public string Comment { get; set; }

        public virtual ICollection<Cloze> Clozes { get; set; }

        public IEnumerable<ICloze> GetClozes() => Clozes;

        public ICloze GetNextCloze() =>
            Clozes.GetMinElement(c => c.Position);

        public void AddCloze(ICloze cloze) =>
            Clozes.Add(cloze as Cloze);

        public bool IsValid { get; set; }

        public bool IsDeleted { get; set; }
    }
}
