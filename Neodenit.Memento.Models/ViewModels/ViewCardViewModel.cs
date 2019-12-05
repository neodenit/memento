using System;
using System.ComponentModel.DataAnnotations;
using Neodenit.Memento.Models.DataModels;

namespace Neodenit.Memento.Models.ViewModels
{
    public class ViewCardViewModel
    {
        public Guid ID { get; set; }

        public Guid DeckID { get; set; }

        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        public string DeckTitle { get; set; }

        public ViewCardViewModel() { }

        public ViewCardViewModel(Card card)
        {
            ID = card.ID;
            DeckID = card.DeckID;
            Text = card.Text;
            DeckTitle = card.Deck.Title;
        }
    }
}
