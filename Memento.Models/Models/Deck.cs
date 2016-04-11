using Memento.Attributes;
using Memento.Common;
using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Memento.Models.Models
{
    [Serializable]
    public class Deck : IDeck
    {
        public Deck()
        {
            Cards = new List<Card>();
        }

        [CheckDeckOwner]
        public int ID { get; set; }

        [Required]
        public string Title { get; set; }

        public string Owner { get; set; }

        public virtual ICollection<Card> Cards { get; set; }

        [Display(Name = "Control Mode")]
        public ControlModes ControlMode { get; set; }

        [Display(Name = "Delay Mode")]
        public DelayModes DelayMode { get; set; }

        [Display(Name = "Allow Small Delays")]
        public bool AllowSmallDelays { get; set; }

        [Display(Name = "Initial Delay")]
        public int StartDelay { get; set; }

        [Display(Name = "Coefficient")]
        public double Coeff { get; set; }

        public IEnumerable<ICloze> GetClozes()
        {
            var validCards = GetValidCards();
            return validCards.SelectMany(card => card.GetClozes() ?? Enumerable.Empty<ICloze>());
        }

        public ICard GetNextCard()
        {
            var validCards = GetValidCards();

            var nextCard = validCards.GetMinElement(item => item.GetNextCloze().Position);

            return nextCard;
        }

        public IEnumerable<ICard> GetAllCards()
        {
            return Cards;
        }

        public IEnumerable<ICard> GetValidCards()
        {
            return Cards.Where(card => card.IsValid && !card.IsDeleted);
        }

        public IEnumerable<ICard> GetDraftCards()
        {
            return Cards.Where(card => !card.IsValid && !card.IsDeleted);
        }

        public IEnumerable<ICard> GetDeletedCards()
        {
            return Cards.Where(card => card.IsDeleted);
        }
    }
}
