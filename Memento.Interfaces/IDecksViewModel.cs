using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface IDecksViewModel
    {
        IEnumerable<IDeck> UserDecks { get; set; }
        IEnumerable<IDeck> SharedDecks { get; set; }
    }
}
