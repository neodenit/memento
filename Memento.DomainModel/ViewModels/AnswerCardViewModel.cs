using Memento.DomainModel.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.DomainModel.ViewModels
{
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
