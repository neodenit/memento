using Memento.DomainModel.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.DomainModel.ViewModels
{
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
}
