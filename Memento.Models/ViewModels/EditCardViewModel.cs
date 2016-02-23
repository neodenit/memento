using Memento.Attributes;
using Memento.Interfaces;
using Memento.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Models.ViewModels
{
    [CheckClozes]
    public class EditCardViewModel : IEditCardViewModel
    {
        public int ID { get; set; }

        public int DeckID { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        public EditCardViewModel() { }

        public EditCardViewModel(ICard card)
        {
            ID = card.ID;
            DeckID = card.DeckID;
            Text = card.Text;
        }
    }
}
