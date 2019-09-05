using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Memento.Attributes;
using Memento.Interfaces;

namespace Memento.Models.ViewModels
{
    [CheckClozes]
    public class EditCardViewModel : IEditCardViewModel
    {
        public Guid ID { get; set; }

        [Display(Name = "Deck")]
        public int DeckID { get; set; }

        public Guid ReadingCardId { get; set; }

        [Required]
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }

        public EditCardViewModel() { }

        public EditCardViewModel(ICard card)
        {
            ID = card.ID;
            DeckID = card.DeckID;
            Text = card.Text;
            Comment = card.Comment;
        }
    }
}
