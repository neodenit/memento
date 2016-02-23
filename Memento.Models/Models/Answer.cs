using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Models.Models
{
    public class Answer : IAnswer
    {
        public int ID { get; set; }

        public DateTime Time { get; set; }

        public string Owner { get; set; }

        public bool IsCorrect { get; set; }

        public int ClozeID { get; set; }

        public int CardID { get; set; }

        public int DeckID { get; set; }

        public int CardsInRepetition { get; set; }
    }
}
