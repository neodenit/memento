using Memento.Attributes;
using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Models.ViewModels
{
    [CheckClozes]
    public class ViewCardViewModel : IViewCardViewModel
    {
        public int ID { get; set; }
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
