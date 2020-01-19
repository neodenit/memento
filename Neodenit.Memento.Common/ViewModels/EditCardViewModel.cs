using System;
using System.ComponentModel.DataAnnotations;

namespace Neodenit.Memento.Common.ViewModels
{
    public class EditCardViewModel
    {
        public Guid ID { get; set; }

        [Display(Name = "Deck")]
        public Guid DeckID { get; set; }

        public string DeckTitle { get; set; }

        public Guid ReadingCardId { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }
    }
}
