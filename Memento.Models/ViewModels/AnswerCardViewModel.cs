using System;
using System.ComponentModel.DataAnnotations;
using Memento.Common.App_GlobalResources;
using Memento.Interfaces;

namespace Memento.Models.ViewModels
{
    public class AnswerCardViewModel : IAnswerCardViewModel
    {
        public Guid ID { get; set; }

        public int DeckID { get; set; }

        public string DeckTitle { get; set; }

        [DataType(DataType.MultilineText)]
        public string Question { get; set; }

        [DataType(DataType.MultilineText)]
        public string FullAnswer { get; set; }

        public string ShortAnswer { get; set; }

        [Required(ErrorMessageResourceName = "PleaseEnterYourAnswer", ErrorMessageResourceType = typeof(Resources))]
        public string UserAnswer { get; set; }

        public string Comment { get; set; }

        public Mark Mark { get; set; }

        public DelayModes DelayMode { get; set; }

        public IDeckWithStatViewModel Statistics { get; set; }

        public AnswerCardViewModel() { }

        public AnswerCardViewModel(ICard card)
        {
            ID = card.ID;
            DeckID = card.DeckID;

            var deck = card.GetDeck();
            DeckTitle = deck.Title;
            DelayMode = deck.DelayMode;
        }
    }
}
