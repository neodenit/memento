using System;
using System.ComponentModel.DataAnnotations;
using Memento.Interfaces;

namespace Memento.Models.ViewModels
{
    public class ViewCardViewModel : IViewCardViewModel
    {
        public Guid ID { get; set; }
        public int DeckID { get; set; }

        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        public string DeckTitle { get; set; }

        public ViewCardViewModel() { }

        public ViewCardViewModel(ICard card)
        {
            ID = card.ID;
            DeckID = card.DeckID;
            Text = card.Text;
            DeckTitle = card.GetDeck().Title;
        }
    }
}
