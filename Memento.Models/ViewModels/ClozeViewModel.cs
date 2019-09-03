using System;
using Memento.Interfaces;

namespace Memento.Models.ViewModels
{
    public class ClozeViewModel
    {
        public int ID { get; set; }

        public Guid CardID { get; set; }

        public string Label { get; set; }

        public int Position { get; set; }

        public bool IsNew { get; set; }

        public int LastDelay { get; set; }

        public int TopicID { get; set; }

        public string Text { get; set; }

        public ClozeViewModel(ICloze cloze, string username, string text = "")
        {
            ID = cloze.ID;
            CardID = cloze.CardID;
            Label = cloze.Label;
            Position = cloze.GetUserRepetition(username).Position;
            IsNew = cloze.GetUserRepetition(username).IsNew;
            LastDelay = cloze.GetUserRepetition(username).LastDelay;
            Text = text;
        }
    }
}
