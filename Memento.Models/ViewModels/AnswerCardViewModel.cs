using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Models.ViewModels
{
    public class AnswerCardViewModel : IAnswerCardViewModel
    {
        public int ID { get; set; }

        public int DeckID { get; set; }

        public string DeckTitle { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        public string Question { get; set; }

        [Required]
        public string Answer { get; set; }

        public Mark Mark { get; set; }

        public AnswerCardViewModel() { }

        public AnswerCardViewModel(ICard card)
        {
            ID = card.ID;
            DeckID = card.DeckID;
            Text = card.Text;
            DeckTitle = card.GetDeck().Title;
        }
    }
}
