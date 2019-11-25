using System;
using Memento.Models.Models;

namespace Memento.Models.ViewModels
{
    public class ClozeViewModel
    {
        public Guid ID { get; set; }

        public Guid CardID { get; set; }

        public string Label { get; set; }

        public int Position { get; set; }

        public bool IsNew { get; set; }

        public int LastDelay { get; set; }

        public int TopicID { get; set; }

        public string Text { get; set; }

        public ClozeViewModel(Cloze cloze, string username, string text = "")
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
