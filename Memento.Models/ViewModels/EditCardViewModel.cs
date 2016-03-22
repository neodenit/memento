using Memento.Attributes;
using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Memento.Models.ViewModels
{
    [CheckClozes]
    public class EditCardViewModel : IEditCardViewModel
    {
        public int ID { get; set; }

        public int DeckID { get; set; }

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
