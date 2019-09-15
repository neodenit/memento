using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Memento.Common;
using Memento.Models.Enums;

namespace Memento.Models.Models
{
    [Serializable]
    public class Deck
    {
        public int ID { get; set; }

        public bool IsShared { get; set; }

        [Required]
        public string Title { get; set; }

        public string Owner { get; set; }

        public virtual ICollection<Card> Cards { get; set; } = new List<Card>();

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

        public bool PreviewAnswer { get; set; }

        public IEnumerable<Cloze> GetClozes()
        {
            var validCards = GetValidCards();
            return validCards.SelectMany(card => card.GetClozes() ?? Enumerable.Empty<Cloze>());
        }

        public IEnumerable<UserRepetition> GetRepetitions(string username)
        {
            var clozes = GetClozes();
            var userRepetitions = from c in clozes select c.GetUserRepetition(username);
            var result = from ur in userRepetitions where ur != null select ur;

            return result;
        }

        public Card GetNextCard(string username)
        {
            var validCards = GetValidCards();

            var nextCard = validCards.GetMinElement(item => item.GetNextCloze(username).GetUserRepetition(username).Position);

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

        public IEnumerable<string> GetUsers() =>
            Cards.SelectMany(x => x.GetUsers()).Distinct();

        public ICollection<UserRepetition> GetRepetitions()
        {
            throw new NotImplementedException();
        }
    }
}
