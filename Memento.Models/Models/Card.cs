using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Memento.Common;

namespace Memento.Models.Models
{
    [Serializable]
    public class Card
    {
        public Card()
        {
            Clozes = new List<Cloze>();
        }

        public Card(Deck deck, string text, string comment, bool isValid) : this()
        {
            Deck = deck as Deck;
            Text = text;
            IsValid = isValid;
            Comment = comment;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ID { get; set; }

        public Guid ReadingCardId { get; set; }

        public int DeckID { get; set; }

        public virtual Deck Deck { get; set; }

        public Deck GetDeck()
        {
            return Deck;
        }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        public string Comment { get; set; }

        public virtual ICollection<Cloze> Clozes { get; set; }

        public IEnumerable<Cloze> GetClozes() => Clozes;

        public Cloze GetNextCloze(string username) =>
            Clozes.GetMinElement(c => c.GetUserRepetition(username).Position);

        public void AddCloze(Cloze cloze) =>
            Clozes.Add(cloze as Cloze);

        public IEnumerable<string> GetUsers() =>
            Clozes.SelectMany(x => x.GetUsers()).Distinct();

        public bool IsValid { get; set; }

        public bool IsDeleted { get; set; }
    }
}
