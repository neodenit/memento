using System;

namespace Memento.Models.Models
{
    public class Answer
    {
        public int ID { get; set; }

        public DateTime Time { get; set; }

        public string Owner { get; set; }

        public bool IsCorrect { get; set; }

        public int ClozeID { get; set; }

        public Guid CardID { get; set; }

        public int DeckID { get; set; }
    }
}
