using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Models.ViewModels
{
    public class DeckViewModel
    {
        public int ID { get; set; }

        public string Title { get; set; }

        public DeckViewModel() { }

        public DeckViewModel(IDeck deck)
        {
            ID = deck.ID;
            Title = deck.Title;
        }
    }
}
