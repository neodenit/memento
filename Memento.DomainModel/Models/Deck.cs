using Memento.Core;
using Memento.DomainModel.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;

namespace Memento.DomainModel.Models
{
    public enum ControlModes
    {
        Automatic,
        Manual,
    }

    public enum DelayModes
    {
        Smooth,
        Sharp,
        Combined,
    }

    public class Deck : IDeck
    {
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

        public IEnumerable<Cloze> GetClozes()
        {
            var validCards = GetValidCards();
            return validCards.SelectMany(card => card.Clozes ?? Enumerable.Empty<Cloze>());
        }

        public Card GetNextCard()
        {
            var validCards = GetValidCards();

            var nextCard = validCards.GetMinElement(item => item.Clozes.Min(c => c.Position));

            return nextCard;
        }

        public IEnumerable<Card> GetAllCards()
        {
            return Cards;
        }

        public IEnumerable<Card> GetValidCards()
        {
            return Cards.Where(card => card.IsValid && !card.IsDeleted);
        }

        public IEnumerable<Card> GetDraftCards()
        {
            return Cards.Where(card => !card.IsValid && !card.IsDeleted);
        }

        public IEnumerable<Card> GetDeletedCards()
        {
            return Cards.Where(card => card.IsDeleted);
        }

        public bool IsAuthorized(IPrincipal user)
        {
            var userName = user.Identity.Name;

            return Owner == userName;
        }
    }
}
