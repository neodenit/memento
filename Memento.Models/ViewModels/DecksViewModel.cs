using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public class DecksViewModel : IDecksViewModel
    {
        public IEnumerable<IDeck> UserDecks { get; set; }
        public IEnumerable<IDeck> SharedDecks { get; set; }
    }
}
