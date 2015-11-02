using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.DomainModel.Models
{
    public class ClozeView
    {
        public int ID { get; set; }

        public int CardID { get; set; }

        public string Label { get; set; }

        public int Position { get; set; }

        public bool IsNew { get; set; }

        public int LastDelay { get; set; }

        public int TopicID { get; set; }

        public string Text { get; set; }

        public ClozeView(Cloze cloze, string text = "")
        {
            ID = cloze.ID;
            CardID = cloze.CardID;
            Label = cloze.Label;
            Position = cloze.Position;
            IsNew = cloze.IsNew;
            LastDelay = cloze.LastDelay;
            Text = text;
        }
    }

    public class EditCardViewModel
    {
        public int ID { get; set; }

        public int DeckID { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        public EditCardViewModel() { }

        public EditCardViewModel(Card card)
        {
            ID = card.ID;
            DeckID = card.DeckID;
            Text = card.Text;
        }
    }

    public class AnswerCardViewModel
    {
        public int ID { get; set; }

        public int DeckID { get; set; }

        public string DeckTitle { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        [Required]
        public string Answer { get; set; }

        public AnswerCardViewModel() { }

        public AnswerCardViewModel(Card card)
        {
            ID = card.ID;
            DeckID = card.DeckID;
            Text = card.Text;
            DeckTitle = card.Deck.Title;
        }
    }
}
