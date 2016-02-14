using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.DomainModel.Models
{
    public class Cloze : ICloze
    {
        public Cloze()
        {
        }

        public Cloze(int cardID, string label)
        {
            CardID = cardID;
            Label = label;
        }

        public int ID { get; set; }

        public int CardID { get; set; }

        public virtual Card Card { get; set; }

        public string Label { get; set; }

        public int Position { get; set; }

        public bool IsNew { get; set; }

        public int LastDelay { get; set; }
    }
}
