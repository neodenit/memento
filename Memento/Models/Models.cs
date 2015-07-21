using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Script.Serialization;
using Memento;
using Memento.SRS;

namespace Memento.Models
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
        public int ID { get; set; }

        [Required]
        public string Title { get; set; }

        public string Owner { get; set; }

        public virtual ICollection<Card> Cards { get; set; }

        public ControlModes ControlMode { get; set; }

        public DelayModes DelayMode { get; set; }

        public bool AllowSmallDelays { get; set; }

        public int StartDelay { get; set; }

        public double Coeff { get; set; }

        public IEnumerable<Cloze> GetClozes()
        {
            return Cards.SelectMany(card => card.Clozes ?? Enumerable.Empty<Cloze>());
        }

        public Card GetNextCard()
        {
            var validCards = GetValidCards();

            var nextCard = validCards.GetMinElement(item => item.Clozes.Min(c => c.Position));

            return nextCard;
        }

        public IEnumerable<Card> GetValidCards()
        {
            return Cards.Where(card => card.IsValid);
        }

        public bool IsAuthorized(IPrincipal user)
        {
            var userName = user.Identity.Name;

            return Owner == userName;
        }
    }

    public class Card
    {
        public int ID { get; set; }

        public int DeckID { get; set; }

        [ScriptIgnore(ApplyToOverrides = true)]
        public virtual Deck Deck { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        public string Answer { get; set; }

        public virtual ICollection<Cloze> Clozes { get; set; }

        public bool IsValid { get; set; }

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

    public class Cloze : ICard
    {
        public Cloze()
        {
        }

        public Cloze(int cardID, string label)
        {
            CardID = cardID;
            Label = label;
        }

        public int ID { get; set; }

        public int CardID { get; set; }

        [ScriptIgnore(ApplyToOverrides = true)]
        public virtual Card Card { get; set; }

        public string Label { get; set; }

        public int Position { get; set; }

        public bool IsNew { get; set; }

        public int LastDelay { get; set; }

        public int TopicID { get { return Card.ID; } }
    }

    public class Answer
    {
        public int ID { get; set; }

        public DateTime Time { get; set; }

        public string Owner { get; set; }

        public bool IsCorrect { get; set; }

        public int ClozeID { get; set; }

        public int CardID { get; set; }

        public int DeckID { get; set; }

        public int CardsInRepetition { get; set; }
    }

    public class DailyRepetitionStat
    {
        public DateTime Date { get; set; }

        public int TotalCount { get; set; }

        public int NewCount { get; set; }
    }
}