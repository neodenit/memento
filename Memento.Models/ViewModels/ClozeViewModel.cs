﻿using Memento.Interfaces;
using Memento.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Models.ViewModels
{
    public class ClozeViewModel
    {
        public int ID { get; set; }

        public int CardID { get; set; }

        public string Label { get; set; }

        public int Position { get; set; }

        public bool IsNew { get; set; }

        public int LastDelay { get; set; }

        public int TopicID { get; set; }

        public string Text { get; set; }

        public ClozeViewModel(ICloze cloze, string text = "")
        {
            ID = cloze.ID;
            CardID = cloze.CardID;
            Label = cloze.Label;
            Position = cloze.Position;
            IsNew = cloze.IsNew;
            LastDelay = cloze.LastDelay;
            Text = text;
        }
    }
}