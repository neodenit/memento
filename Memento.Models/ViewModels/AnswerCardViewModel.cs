using Memento.Common.App_GlobalResources;
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

        public string DeckTitle { get; set; }

        [DataType(DataType.MultilineText)]
        public string Question { get; set; }

        [DataType(DataType.MultilineText)]
        public string FullAnswer { get; set; }

        public string CorrectAnswer { get; set; }

        [Required(ErrorMessageResourceName = "PleaseEnterYourAnswer", ErrorMessageResourceType = typeof(Resources))]
        public string UserAnswer { get; set; }

        public Mark Mark { get; set; }

        public DelayModes DelayMode { get; set; }

        public AnswerCardViewModel() { }

        public AnswerCardViewModel(ICard card)
        {
            ID = card.ID;

            var deck = card.GetDeck();
            DeckTitle = deck.Title;
            DelayMode = deck.DelayMode;
        }
    }
}
