﻿using System;
using System.ComponentModel.DataAnnotations;
using Neodenit.Memento.Common.App_GlobalResources;
using Neodenit.Memento.Models.DataModels;
using Neodenit.Memento.Models.Enums;

namespace Neodenit.Memento.Models.ViewModels
{
    public class AnswerCardViewModel
    {
        public Guid ID { get; set; }

        public Guid DeckID { get; set; }

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

        public DeckWithStatViewModel Statistics { get; set; }

        public AnswerCardViewModel() { }

        public AnswerCardViewModel(Card card)
        {
            ID = card.ID;
            DeckID = card.DeckID;

            var deck = card.Deck;
            DeckTitle = deck.Title;
            DelayMode = deck.DelayMode;
        }
    }
}