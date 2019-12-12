using System;

namespace Neodenit.Memento.Models.ViewModels
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
    }
}
